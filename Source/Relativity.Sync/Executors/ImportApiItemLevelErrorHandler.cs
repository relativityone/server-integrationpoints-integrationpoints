using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Import.V1;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class ImportApiItemLevelErrorHandler : ItemLevelErrorHandlerBase, IImportApiItemLevelErrorHandler
    {
        public ImportApiItemLevelErrorHandler(IItemLevelErrorHandlerConfiguration configuration, IJobHistoryErrorRepository jobHistoryErrorRepository)
        : base(configuration, jobHistoryErrorRepository)
        {
        }

        public async Task HandleItemLevelErrorsAsync(
            IImportSourceController sourceController,
            IBatch batch,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            if (batch.Status.IsIn(BatchStatus.CompletedWithErrors, BatchStatus.Cancelled, BatchStatus.Failed))
            {
                ImportErrors itemLevelErrors = await GetItemLevelErrorsAsync(sourceController, configuration, batch.BatchGuid);
                List<ErrorDetail> errorDetails = itemLevelErrors.Errors.SelectMany(x => x.ErrorDetails).ToList();

                foreach (ErrorDetail errorDetail in errorDetails)
                {
                    CreateItemLevelErrorInJobHistory(errorDetail);
                }
            }
        }

        private async Task<ImportErrors> GetItemLevelErrorsAsync(
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
                    "Unable to retrieve Item Level Errors. DestinationWorkspaceArtifactId - {0}, jobId - {1}, batchId - {2}, ErrorCode - {3}, ErrorMessage - {4}",
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId,
                    batchId,
                    itemLevelErrorsResponse.ErrorCode,
                    itemLevelErrorsResponse.ErrorMessage);
                throw new Exception(message);
            }

            ImportErrors itemLevelErrors = itemLevelErrorsResponse.Value;
            return itemLevelErrors;
        }

        private void CreateItemLevelErrorInJobHistory(
            ErrorDetail errorDetail)
        {
            string errorMessage = errorDetail.ErrorMessage;
            bool isIdentifierReturned = errorDetail
                .ErrorProperties
                .TryGetValue("Identifier", out string identifier);

            if (!isIdentifierReturned)
            {
                identifier = $"Unknown identifier - {Guid.NewGuid()}";
                errorMessage = $"It was impossible to determine document identifier. ErrorMessage: {errorMessage}";
            }

            ItemLevelError itemLevelError = new ItemLevelError(identifier, errorMessage);
            HandleBatchItemErrors(itemLevelError);
        }
    }
}
