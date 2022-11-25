using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.DataExchange.Export.VolumeManagerV2;
using Relativity.Import.V1.Models.Errors;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandler_TEMP : ItemLevelErrorHandlerBase, IItemLevelErrorHandler_TEMP
    {
        private readonly IItemStatusMonitor _statusMonitor;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;

        public ItemLevelErrorHandler_TEMP(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository, IItemStatusMonitor statusMonitor, IAPILog logger)
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

    internal class ItemLevelErrorHandler : IItemLevelErrorHandler
    {
        private const int _ITEM_LEVEL_ERRORS_CREATE_BATCH_SIZE = 10000;

        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;

        private ConcurrentQueue<CreateJobHistoryErrorDto> _itemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();

        public ItemLevelErrorHandler(
            IItemLevelErrorHandlerConfiguration configuration,
            IJobHistoryErrorRepository jobHistoryErrorRepository,
            IItemLevelErrorLogAggregator itemLevelErrorLogAggregator)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _itemLevelErrorLogAggregator = itemLevelErrorLogAggregator;
        }

        public void HandleIAPIItemLevelErrors(IEnumerable<ImportErrors> errors)
        {
            throw new System.NotImplementedException();
        }

        public void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            HandleBatchItemErrorAsync(itemLevelError)
                .GetAwaiter().GetResult();
        }

        public async Task HandleRemainingErrorsAsync()
        {
            if (_itemErrors.Any())
            {
                await CreateErrorsAsync().ConfigureAwait(false);
            }

            await _itemLevelErrorLogAggregator.LogAllItemLevelErrorsAsync().ConfigureAwait(false);
        }

        private async Task HandleBatchItemErrorAsync(ItemLevelError itemLevelError)
        {
            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            _itemErrors.Enqueue(itemError);

            if (_itemErrors.Count >= _ITEM_LEVEL_ERRORS_CREATE_BATCH_SIZE)
            {
                await CreateErrorsAsync().ConfigureAwait(false);
            }
        }

        private async Task CreateErrorsAsync()
        {
            int currentNumberOfItemLevelErrorsInQueue = _itemErrors.Count;
            List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(currentNumberOfItemLevelErrorsInQueue);
            for (int i = 0; i < currentNumberOfItemLevelErrorsInQueue; i++)
            {
                if (_itemErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
                {
                    itemLevelErrors.Add(dto);
                }
            }

            if (itemLevelErrors.Any())
            {
                await _jobHistoryErrorRepository.MassCreateAsync(
                        _configuration.SourceWorkspaceArtifactId,
                        _configuration.JobHistoryArtifactId,
                        itemLevelErrors)
                    .ConfigureAwait(false);
            }
        }
    }
}
