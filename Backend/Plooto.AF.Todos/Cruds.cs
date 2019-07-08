using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Plooto.AF.Todos.Dtos;
using Plooto.AF.Todos.Models;
using Plooto.Extensions.AzureSearch;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Plooto.AF.Todos
{
    public class TodosApi
    {
        [FunctionName(nameof(GetTodos))]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")]
            HttpRequest req,
            [AzureSearch] ISearchIndexClient todo,
            ILogger log)
        {
            var docs = await todo.Documents.SearchAsync<Ticket>(
                "*",
                new SearchParameters
                {
                    IncludeTotalResultCount = true
                });

            return new OkObjectResult(new
            {
                tickets = docs.Results.Select(r => r.Document),
                docs.ContinuationToken,
                docs.Count
            });
        }

        [FunctionName(nameof(GetTodosSuggestion))]
        public static async Task<IActionResult> GetTodosSuggestion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/suggestion")]
            HttpRequest req,
            [AzureSearch] ISearchIndexClient todos,
            ILogger log)
        {
            return new OkObjectResult("all todos");
        }


        [FunctionName(nameof(GetTodoById))]
        public static async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")]
            HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult(nameof(GetTodoById));
        }

        [FunctionName(nameof(PostTodo))]
        public async Task<IActionResult> PostTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")]
            HttpRequest req,
            [AzureSearch(ApiKey = "AzureSearch:WriteApiKey")] IAsyncCollector<Ticket> collector,
            ILogger logger)
        {
            var newTicket = JsonConvert.DeserializeObject<NewTicket>(await req.ReadAsStringAsync().ConfigureAwait(false));

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

            try
            {
                var ticket = new Ticket
                {
                    Description = newTicket.Description,
                    Tags = newTicket.Tags?.ToArray() ?? new string[]{},
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
            catch (Exception e)
            {
                logger.LogError($"{nameof(PostTodo)}", e);
                return new UnprocessableEntityObjectResult(e);
            }
        }

        [FunctionName(nameof(PutTodoById))]
        public static async Task<IActionResult> PutTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")]
            HttpRequest req,
            ILogger log)
        {
            return new NoContentResult();
        }

        [FunctionName(nameof(DeleteTodoById))]
        public static async Task<IActionResult> DeleteTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")]
            HttpRequest req,
            ILogger log)
        {
            return new NoContentResult();
        }
    }
}
