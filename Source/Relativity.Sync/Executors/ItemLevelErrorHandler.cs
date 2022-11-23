using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Shared.V1.Exceptions;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ItemLevelErrorHandler : IItemLevelErrorHandler
    {
        private const int _BATCH_ITEM_ERRORS_COUNT_FOR_RDO_CREATE = 1000;

        private readonly IItemLevelErrorHandlerConfiguration _configuration;
        private readonly IItemLevelErrorLogAggregator _itemLevelErrorLogAggregator;
        private readonly IJobHistoryErrorRepository _jobHistoryErrorRepository;

        private ConcurrentQueue<CreateJobHistoryErrorDto> _batchItemErrors;
        private IItemStatusMonitor _statusMonitor;

        public ItemLevelErrorHandler(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository, IItemStatusMonitor statusMonitor, IAPILog logger)
        {
            _configuration = configuration;
            _jobHistoryErrorRepository = jobHistoryErrorRepository;
            _itemLevelErrorLogAggregator = new ItemLevelErrorLogAggregator(logger);
            _batchItemErrors = new ConcurrentQueue<CreateJobHistoryErrorDto>();
            _statusMonitor = statusMonitor;
        }

        public void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError)
        {
            MarkItemAsFailed(itemLevelError);

            CreateJobHistoryErrorDto itemError = new CreateJobHistoryErrorDto(ErrorType.Item)
            {
                ErrorMessage = itemLevelError.Message,
                SourceUniqueId = itemLevelError.Identifier
            };

            _batchItemErrors.Enqueue(itemError);

            if (_batchItemErrors.Count >= _BATCH_ITEM_ERRORS_COUNT_FOR_RDO_CREATE)
            {
                CreateJobHistoryErrors();
            }
        }

        public async Task HandleIApiItemLevelErrors(
            IImportSourceController sourceController,
            List<IBatch> batches,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            foreach (IBatch batch in batches)
            {
                ImportErrors itemLevelErrors = await GetItemLevelErrors(sourceController, configuration, batch.BatchGuid);
                DataSourceDetails dataSourceDetails = await GetDataSourceDetails(sourceController, configuration, batch.BatchGuid);
                foreach (ImportError error in itemLevelErrors.Errors)
                {
                    int lineNumber = error.LineNumber;
                    foreach (ErrorDetail errorDetail in error.ErrorDetails)
                    {
                        CreateItemLevelErrorInJobHistory(errorDetail, dataSourceDetails, lineNumber);
                    }
                }
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

        private async Task<DataSourceDetails> GetDataSourceDetails(
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration,
            Guid batchId)
        {
            ValueResponse<DataSourceDetails> dataSourceResponse = await sourceController.GetDetailsAsync(
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId,
                    batchId)
                .ConfigureAwait(false);
            if (!dataSourceResponse.IsSuccess)
            {
                string message = string.Format(
                    "Unable to retrieve Data source details. DestinationWorkspaceArtifactId - {0}, jobId - {1}, batchId - {2}",
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId,
                    batchId);
                throw new NotFoundException(message);
            }

            DataSourceDetails dataSourceDetails = dataSourceResponse.Value;
            return dataSourceDetails;
        }

        private async Task<ImportErrors> GetItemLevelErrors(
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration,
            Guid batchId)
        {
            ValueResponse<ImportErrors> itemLevelErrorsResponse = await sourceController
                .GetItemErrorsAsync(
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId,
                    batchId,
                    0,
                    int.MaxValue)
                .ConfigureAwait(false);
            if (!itemLevelErrorsResponse.IsSuccess)
            {
                string message = string.Format(
                    "Unable to retrieve Item Level Errors. DestinationWorkspaceArtifactId - {0}, jobId - {1}, batchId - {2}",
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId,
                    batchId);
                throw new NotFoundException(message);
            }

            ImportErrors itemLevelErrors = itemLevelErrorsResponse.Value;
            return itemLevelErrors;
        }

        private void CreateItemLevelErrorInJobHistory(
            ErrorDetail errorDetail,
            DataSourceDetails dataSourceDetails,
            int lineNumber)
        {
            string errorMessage = errorDetail.ErrorMessage;
            bool isIdentifierReturned = errorDetail
                .ErrorProperties
                .TryGetValue("Identifier", out string identifier);

            if (!isIdentifierReturned)
            {
                string itemLevelErrorRow = File
                    .ReadLines(dataSourceDetails.DataSourceSettings.Path)
                    .Skip(lineNumber)
                    .Take(1)
                    .First();
                const int identifierColumnIndex = 0;
                identifier = itemLevelErrorRow.Split(dataSourceDetails.DataSourceSettings.ColumnDelimiter)[identifierColumnIndex];
            }

            ItemLevelError itemLevelError = new ItemLevelError(identifier, errorMessage);
            HandleItemLevelError(0, itemLevelError);
        }

        private void MarkItemAsFailed(ItemLevelError itemLevelError)
        {
            int itemArtifactId = _statusMonitor.GetArtifactId(itemLevelError.Identifier);
            _itemLevelErrorLogAggregator.AddItemLevelError(itemLevelError, itemArtifactId);
            _statusMonitor.MarkItemAsFailed(itemLevelError.Identifier);
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
