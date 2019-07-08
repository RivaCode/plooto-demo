using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Plooto.Extensions.AzureSearch.Bindings
{
    internal class DocumentCollector<T> : IAsyncCollector<T>
    {
        private readonly ISearchIndexClient _searchIndexClient;
        private readonly ILogger _logger;
        private IEnumerable<T> _collection;

        public DocumentCollector(
            ISearchIndexClient searchIndexClient,
            ILogger logger)
        {
            _searchIndexClient = searchIndexClient;
            _logger = logger;
            Flush();
        }

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{nameof(AddAsync)}: adding new item (IndexName: {_searchIndexClient.IndexName})");
            _collection = _collection.Append(item);
            return Task.CompletedTask;
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            var documents = _collection.ToList();
            if (!documents.Any())
            {
                _logger.LogWarning($"{nameof(FlushAsync)}: no items to flush (IndexName: {_searchIndexClient.IndexName})");
                return;
            }

            await _searchIndexClient.Documents.IndexAsync(
                IndexBatch.Upload(documents),
                cancellationToken: cancellationToken);

            _logger.LogInformation($"{nameof(FlushAsync)}: all items uploaded (IndexName: {_searchIndexClient.IndexName})");
            Flush();
        }

        private void Flush() => _collection = Enumerable.Empty<T>();
    }
}