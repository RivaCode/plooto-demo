using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Plooto.Extensions.HttpQuery;
using Plooto.Extensions.HttpQuery.Config;

[assembly: WebJobsStartup(typeof(HttpWebJobStartup))]
namespace Plooto.Extensions.HttpQuery
{
    public class HttpWebJobStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) => builder.AddHttpQuery();
    }
}
