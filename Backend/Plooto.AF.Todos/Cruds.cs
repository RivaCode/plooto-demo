using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;
using Newtonsoft.Json;
using Plooto.AF.Todos.Dtos;
using Plooto.AF.Todos.Extensions;
using Plooto.AF.Todos.Models;
using Plooto.Extensions.AzureSearch;
using Plooto.Extensions.HttpQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Plooto.AF.Todos
{
    public class TodosApi
    {
        [FunctionName(nameof(GetTodos))]
        public async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequest req,
            [HttpQuery] TicketQuery query,
            [AzureSearch] ISearchIndexClient todo,
            ILogger logger)
        {
            try
            {
                var searchParams = new SearchParameters
                {
                    IncludeTotalResultCount = true,
                    Filter = query.Filter,
                    Select = query.Fields?.Split(",").Append(nameof(Ticket.Id).ToLower()).Distinct().ToArray(),
                    Skip = query.PageSize * (query.PageNumber - 1),
                    Top = query.PageSize,
                };
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug($@"Searching {
                        JsonConvert.SerializeObject(new
                        {
                            SelectedFields = searchParams.Select,
                            searchParams.Filter,
                            searchParams.Skip,
                            Take = searchParams.Top
                        }, Formatting.Indented)}");
                }

                var docs = await todo.Documents
                    .SearchAsync<Ticket>(
                        searchText: "*",
                        searchParameters: searchParams)
                    .ConfigureAwait(false);

                string Route(string queryString) => queryString != null ? $"/api/todos?{queryString}" : null;

                // ReSharper disable once PossibleInvalidOperationException - cannot happen
                var totalPages = (int)Math.Ceiling(decimal.Divide(docs.Count.Value, query.PageSize));
                return new OkObjectResult(new
                {
                    tickets = docs.Results.Select(r => r.Document),
                    docs.ContinuationToken,
                    Pagination = new
                    {
                        totalPages,
                        Previous = Route(query.PreviousPage()),
                        Next = Route(query.NextPage(totalPages)),
                        First = Route(query.FirstPage()),
                        Last = Route(query.LastPage(totalPages))
                    }
                }).StripNull();
            }
            catch (CloudException e) when (e.Response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new BadRequestObjectResult(new { error = e.Message });
            }
        }

        [FunctionName(nameof(GetTodosTags))]
        public async Task<IActionResult> GetTodosTags(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/tags")] HttpRequest req,
            [AzureSearch] ISearchIndexClient todo,
            ILogger log)
        {
            var docs = await todo.Documents
                .SearchAsync<Ticket>(
                    searchText: "*",
                    new SearchParameters(facets: new List<string> {nameof(Ticket.Tags)}))
                .ConfigureAwait(false);

            return new OkObjectResult(
                docs.Facets[nameof(Ticket.Tags)]
                    .Select(facet => new { Tag = facet.Value, facet.Count }));
        }

        [FunctionName(nameof(GetTodosSuggestion))]
        public IActionResult GetTodosSuggestion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/suggestion")] HttpRequest req,
            [AzureSearch] ISearchIndexClient todos,
            ILogger log)
        {
            return new OkObjectResult("all todos");
        }


        [FunctionName(nameof(GetTodoById))]
        public IActionResult GetTodoById(
            /*
             * using route constrains for ambiguous routes
             * https://github.com/MicrosoftDocs/azure-docs/issues/11755#issuecomment-405651650
             */
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id:guid}")] HttpRequest req,
            [AzureSearch(Key = "{id}")] Ticket ticket,
            ILogger logger)
        {
            if (ticket == null)
            {
                logger.LogWarning("Ticket not found");
                return new NotFoundResult();
            }
            logger.LogInformation("Ticket found");
            return new OkObjectResult(ticket);
        }

        [FunctionName(nameof(PostTodo))]
        public async Task<IActionResult> PostTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")] HttpRequest req,
            [AzureSearch(ApiKey = "AzureSearch:WriteApiKey")] IAsyncCollector<Ticket> collector,
            ILogger logger)
        {
            var newTicket = await req.ReadAsAsync<NewTicket>().ConfigureAwait(false);

            if (newTicket == null)
            {
                return new BadRequestObjectResult(new { error = "Must provide ticket object {'description':string, 'tags'?:string[]}" });
            }

            bool IsEmpty(string v) => string.IsNullOrEmpty(v) || string.IsNullOrWhiteSpace(v);

            if (IsEmpty(newTicket.Description))
            {
                return new BadRequestObjectResult(new { error = "Must supply description" });
            }

            if (newTicket.Tags?.Any(IsEmpty) ?? false)
            {
                return new BadRequestObjectResult(new { error = "All tags must supply value" });
            }

            var ticket = new Ticket
            {
                Description = newTicket.Description,
                Tags = newTicket.Tags?.ToArray() ?? new string[] { },
                Completed = false,
                Id = Guid.NewGuid().ToString()
            };
            ticket.Created = ticket.LastUpdate = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace($"{nameof(PostTodo)}: new ticket  {JsonConvert.SerializeObject(ticket, Formatting.Indented)}");
            }
            await collector.AddAsync(ticket).ConfigureAwait(false);
            logger.LogInformation($"{nameof(PostTodo)}: new ticket created (id:${ticket.Id})");

            return new CreatedResult($"todos/{ticket.Id}", ticket);
        }

        [FunctionName(nameof(PutTodoById))]
        public IActionResult PutTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")] HttpRequest req,
            ILogger log)
        {
            return new NoContentResult();
        }

        [FunctionName(nameof(DeleteTodoById))]
        public async Task<IActionResult> DeleteTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")] HttpRequest req,
            string id,
            [AzureSearch(ApiKey = "AzureSearch:WriteApiKey")] ISearchIndexClient todos,
            ILogger logger)
        {
            try
            {
                var ticket = await todos.Documents.GetAsync<Ticket>(id).ConfigureAwait(false);

                logger.LogDebug($"Deleting ticket (id: {id})");
                await todos.Documents.IndexAsync(IndexBatch.Delete(new[] { ticket })).ConfigureAwait(false);

                logger.LogDebug($"Successfully deleted ticket (id: {id})");
                return new NoContentResult();
            }
            catch (CloudException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning($"Ticket not found (id: {id})");
                return new NotFoundObjectResult(new { error = $"Ticket not found (id: {id})", id });
            }
        }
    }
}
