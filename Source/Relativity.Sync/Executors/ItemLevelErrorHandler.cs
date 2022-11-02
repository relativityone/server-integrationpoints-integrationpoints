using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandler : IItemLevelErrorHandler
    {
        private const int _BATCH_ITEM_ERRORS_MAX_COUNT_FOR_RDO_CREATE = 1000;

        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

        private ConcurrentQueue<CreateJobHistoryErrorDto> _batchItemErrors;
        private IItemStatusMonitor _statusMonitor;

        public ItemLevelErrorHandler(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository, IAPILog logger)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _itemLevelErrorLogAggregator = new ItemLevelErrorLogAggregator(logger);
        }

        public void Initialize(IItemStatusMonitor statusMonitor)
        {
            if (_batchItemErrors != null && _batchItemErrors.Any())
            {
                throw new SyncException("Unhandled item level errors from previous batch were found - error collection is not empty.");
            }

            _batchItemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();
            _statusMonitor = statusMonitor;
        }

        public void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            int itemArtifactId = _statusMonitor.GetArtifactId(itemLevelError.Identifier);
            _itemLevelErrorLogAggregator.AddItemLevelError(itemLevelError, itemArtifactId);
            _statusMonitor.MarkItemAsFailed(itemLevelError.Identifier);

            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            _batchItemErrors.Enqueue(itemError);

            if (_batchItemErrors.Count >= _BATCH_ITEM_ERRORS_MAX_COUNT_FOR_RDO_CREATE)
            {
                CreateJobHistoryErrors();
            }
        }

        public async Task HandleDataSourceProcessingFinishedAsync(IBatch batch)
        {
            if (_batchItemErrors.Any())
            {
                CreateJobHistoryErrors();
            }

            await batch.SetFailedDocumentsCountAsync(_statusMonitor.FailedItemsCount).ConfigureAwait(false);
            await _itemLevelErrorLogAggregator.LogAllItemLevelErrorsAsync().ConfigureAwait(false);
        }

        private void CreateJobHistoryErrors()
        {
            int currentNumberOfItemLevelErrorsInQueue = _batchItemErrors.Count;
            List<CreateJobHistoryErrorDto> itemLevelErrors = new List<CreateJobHistoryErrorDto>(currentNumberOfItemLevelErrorsInQueue);
            for (int i = 0; i < currentNumberOfItemLevelErrorsInQueue; i++)
            {
                if (_batchItemErrors.TryDequeue(out CreateJobHistoryErrorDto dto))
                {
                    itemLevelErrors.Add(dto);
                }
            }

            if (itemLevelErrors.Any())
            {
                _jobHistoryErrorRepository.MassCreateAsync(_configuration.SourceWorkspaceArtifactId, _configuration.JobHistoryArtifactId, itemLevelErrors)
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}
