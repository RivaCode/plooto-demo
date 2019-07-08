using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;

namespace Plooto.Extensions.AzureSearch.Config
{
    internal static class AzureSearchWebJobBuilderExtensions
    {
        public static IWebJobsBuilder AddAzureSearch(this IWebJobsBuilder @this)
        {
            @this.Services.AddLazyCache();
            @this.AddExtension<AzureSearchExtensionConfigProvider>();
            return @this;
        }
    }
}
