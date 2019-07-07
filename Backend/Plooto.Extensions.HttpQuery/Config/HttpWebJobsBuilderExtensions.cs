using Microsoft.Azure.WebJobs;

namespace Plooto.Extensions.HttpQuery.Config
{
    internal static class HttpQueryWebJobsBuilderExtensions
    {
        public static IWebJobsBuilder AddHttpQuery(this IWebJobsBuilder @this)
        {
            return @this;
        }
    }
}
