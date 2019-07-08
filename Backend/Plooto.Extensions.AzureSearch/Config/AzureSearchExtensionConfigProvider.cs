using System;
using System.Threading;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Azure.Search;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;
using Plooto.Extensions.AzureSearch.Bindings;

namespace Plooto.Extensions.AzureSearch.Config
{
    [Extension(name: "AzureSearch")]
    internal class AzureSearchExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly ILogger _logger;
        private readonly IAppCache _cacheService;

        public AzureSearchExtensionConfigProvider(
            IAppCache cacheService,
            ILoggerFactory factory)
        {
            _cacheService = cacheService; _logger = factory.CreateLogger(
                LogCategories.CreateTriggerCategory("AzureSearch"));
        }

        public void Initialize(ExtensionConfigContext context)
        {
            var searchRule = context.AddBindingRule<AzureSearchAttribute>();

            searchRule.AddValidator(ValidateAttribute);

            searchRule.BindToInput(SearchIndexClientInput);
            searchRule.BindToCollector<OpenType>(typeof(DocumentConverter<>), constructorArgs: this);
        }

        private ISearchIndexClient SearchIndexClientInput(AzureSearchAttribute input)
        {
            string GenerateKey() => $"{input.SearchServiceName}_{input.ApiKey}";

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    $"Resolving {nameof(ISearchIndexClient)}, AzureSearch:(Service:{input.SearchServiceName}, Index:{input.IndexName})");
            }

            var searchServiceClient = _cacheService.GetOrAdd(
                key: GenerateKey(),
                addItemFactory: () =>
                    new SearchServiceClient(input.SearchServiceName, new SearchCredentials(input.ApiKey)));


            return searchServiceClient.Indexes.GetClient(input.IndexName);
        }

        private static void ValidateAttribute(AzureSearchAttribute input, Type _)
        {
            bool IsEmpty(string value) =>
                string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);

            string Invalid(string propName) =>
                $"{propName} must be provided either by AppSettings or {nameof(AzureSearchAttribute)}";

            if (IsEmpty(input.SearchServiceName))
            {
                throw new InvalidOperationException(Invalid(nameof(AzureSearchAttribute.SearchServiceName)));
            }

            if (IsEmpty(input.ApiKey))
            {
                throw new InvalidOperationException(Invalid(nameof(AzureSearchAttribute.ApiKey)));
            }

            if (IsEmpty(input.IndexName))
            {
                throw new InvalidOperationException(Invalid(nameof(AzureSearchAttribute.IndexName)));
            }
        }

        private class DocumentConverter<T> : IAsyncConverter<AzureSearchAttribute, IAsyncCollector<T>>
        {
            private readonly AzureSearchExtensionConfigProvider _owner;

            public DocumentConverter(AzureSearchExtensionConfigProvider owner) => _owner = owner;

            public Task<IAsyncCollector<T>> ConvertAsync(AzureSearchAttribute input, CancellationToken _) =>
                Task.FromResult<IAsyncCollector<T>>(
                    new DocumentCollector<T>(
                        _owner.SearchIndexClientInput(input),
                        _owner._logger));
        }

    }
}
