using Microsoft.Azure.WebJobs;

namespace Plooto.Extensions.AzureSearch.Config
{
    internal static class AzureSearchWebJobBuilderExtensions
    {
        public static IWebJobsBuilder AddAzureSearch(this IWebJobsBuilder @this)
        {
            return @this;
        }
    }
}
