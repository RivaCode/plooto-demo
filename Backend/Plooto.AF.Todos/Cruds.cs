using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Plooto.AF.Todos
{
    public static class Function1
    {
        [FunctionName(nameof(GetTodos))]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("all todos");
        }

        [FunctionName(nameof(GetTodosSuggestion))]
        public static async Task<IActionResult> GetTodosSuggestion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/suggestion")] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("all todos");
        }


        [FunctionName(nameof(GetTodoById))]
        public static async Task<IActionResult> GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult(nameof(GetTodoById));
        }

        [FunctionName(nameof(PostTodo))]
        public static async Task<IActionResult> PostTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")] HttpRequest req,
            ILogger log)
        {
            return new CreatedResult("", nameof(PostTodo));
        }

        [FunctionName(nameof(PutTodoById))]
        public static async Task<IActionResult> PutTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")] HttpRequest req,
            ILogger log)
        {
            return new NoContentResult();
        }

        [FunctionName(nameof(PutTodoByIdAssign))]
        public static async Task<IActionResult> PutTodoByIdAssign(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}/assign")] HttpRequest req,
        ILogger log)
        {
            return new NoContentResult();
        }

        [FunctionName(nameof(DeleteTodoById))]
        public static async Task<IActionResult> DeleteTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")] HttpRequest req,
            ILogger log)
        {
            return new NoContentResult();
        }

        [FunctionName(nameof(GetUsers))]
        public static async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req,
        ILogger log)
        {
            return new OkObjectResult("all users");
        }
    }
}
