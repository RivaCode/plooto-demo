using Microsoft.Azure.WebJobs.Host.Config;

namespace Plooto.Extensions.HttpQuery.Config
{
    internal class HttpQueryExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context) =>
            context
                .AddBindingRule<HttpQueryAttribute>()
                .Bind(new HttpQueryBindingProvider());
    }
}
