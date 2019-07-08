using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Buffers;

namespace Plooto.AF.Todos.Extensions
{
    internal static class ActionResultExtensions
    {
        private static readonly IOutputFormatter _noNullJsonFormatter = new JsonOutputFormatter(
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }, ArrayPool<char>.Create());

        public static IActionResult StripNull(this ObjectResult @this)
        {
            @this.Formatters.RemoveType<JsonOutputFormatter>();
            @this.Formatters.Add(_noNullJsonFormatter);

            return @this;
        }
    }
}
