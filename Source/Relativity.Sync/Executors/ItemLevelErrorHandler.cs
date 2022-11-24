using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandler : ItemLevelErrorHandlerBase, IItemLevelErrorHandler
    {
        private readonly IItemStatusMonitor _statusMonitor;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;

        public ItemLevelErrorHandler(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository, IItemStatusMonitor statusMonitor, IAPILog logger)
            : base(configuration, jobHistoryErrorRepository)
        {
            _statusMonitor = statusMonitor;
            _itemLevelErrorLogAggregator = new ItemLevelErrorLogAggregator(logger);
        }

        public void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            MarkItemAsFailed(itemLevelError);
            HandleBatchItemErrors(itemLevelError);
        }

        public async Task HandleDataSourceProcessingFinishedAsync(IBatch batch)
        {
            if (BatchItemErrors.Any())
            {
                CreateJobHistoryErrors();
            }

            await batch.SetFailedDocumentsCountAsync(_statusMonitor.FailedItemsCount).ConfigureAwait(false);
            await _itemLevelErrorLogAggregator.LogAllItemLevelErrorsAsync().ConfigureAwait(false);
        }

        private void MarkItemAsFailed(ItemLevelError itemLevelError)
        {
            int itemArtifactId = _statusMonitor.GetArtifactId(itemLevelError.Identifier);
            _itemLevelErrorLogAggregator.AddItemLevelError(itemLevelError, itemArtifactId);
            _statusMonitor.MarkItemAsFailed(itemLevelError.Identifier);
        }
    }
}
