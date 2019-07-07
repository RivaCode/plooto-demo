using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Plooto.Extensions.AzureSearch;
using Plooto.Extensions.AzureSearch.Config;

[assembly: WebJobsStartup(typeof(AzureSearchWebJobStartup))]
namespace Plooto.Extensions.AzureSearch
{
    public class AzureSearchWebJobStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) => builder.AddAzureSearch();
    }
}
