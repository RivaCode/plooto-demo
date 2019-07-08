using Microsoft.Azure.WebJobs.Description;
using System;

namespace Plooto.Extensions.AzureSearch
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    [Binding]
    public class AzureSearchAttribute : Attribute
    {
        [AppSetting(Default = "AzureSearch:IndexName")]
        public string IndexName { get; set; }

        [AppSetting(Default = "AzureSearch:SearchServiceName")]
        public string SearchServiceName { get; set; }

        [AppSetting(Default = "AzureSearch:ApiKey")]
        public string ApiKey { get; set; }

        [AutoResolve]
        public string Key { get; set; }
    }
}
