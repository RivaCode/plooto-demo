using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Plooto.AF.Todos.Extensions
{
    internal static class JsonExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpRequest @this)
        {
            return JsonConvert.DeserializeObject<T>(
                await @this.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}
