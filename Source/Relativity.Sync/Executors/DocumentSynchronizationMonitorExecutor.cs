using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Errors;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationMonitorExecutor : IExecutor<IDocumentSynchronizationMonitorConfiguration>
    {
        private readonly TimeSpan _DEFAULT_STATUS_CHECK_DELAY = TimeSpan.FromSeconds(10);

        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IProgressHandler _progressHandler;
        private readonly IBatchRepository _batchRepository;
        private readonly IAPILog _logger;
        private readonly IItemLevelErrorHandler _itemLevelErrorHandler;
        private readonly IInstanceSettings _instanceSettings;

        public DocumentSynchronizationMonitorExecutor(
            IDestinationServiceFactoryForUser serviceFactory,
            IProgressHandler progressHandler,
            IItemLevelErrorHandler itemLevelErrorHandler,
            IBatchRepository batchRepository,
            IInstanceSettings instanceSettings,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _progressHandler = progressHandler;
            _itemLevelErrorHandler = itemLevelErrorHandler;
            _batchRepository = batchRepository;
            _instanceSettings = instanceSettings;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IDocumentSynchronizationMonitorConfiguration configuration, CompositeCancellationToken token)
        {
            try
            {
                using (await _progressHandler.AttachAsync(
                    configuration.SourceWorkspaceArtifactId,
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.JobHistoryArtifactId,
                    configuration.ExportRunId,
                    configuration.SyncConfigurationArtifactId)
                           .ConfigureAwait(false))
                {
                    List<IBatch> batches = (await _batchRepository.GetAllAsync(
                            configuration.SourceWorkspaceArtifactId,
                            configuration.SyncConfigurationArtifactId,
                            configuration.ExportRunId)
                        .ConfigureAwait(false))
                        .ToList();

                    _logger.LogInformation(
                        "Retrieved batches to monitor: {@batches}",
                        string.Join(",\n", batches.Select(x => $"{x.BatchGuid} - {x.Status}")));

                    TimeSpan statusCheckDelay = await _instanceSettings
                        .GetImportAPIStatusCheckDelayAsync(_DEFAULT_STATUS_CHECK_DELAY)
                        .ConfigureAwait(false);

                    ImportDetails result;
                    ImportState state = ImportState.Unknown;
                    do
                    {
                        using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                        {
                            if (token.IsStopRequested)
                            {
                                await CancelImportJob(configuration, jobController).ConfigureAwait(false);
                            }

                            if (token.IsDrainStopRequested)
                            {
                                return ExecutionResult.Paused();
                            }

                            await Task.Delay(statusCheckDelay).ConfigureAwait(false);

                            await HandleDataSourceStatusAsync(batches, configuration).ConfigureAwait(false);

                            result = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);
                            if (result.State != state)
                            {
                                state = result.State;
                                _logger.LogInformation("Import status: {@status}", result);
                            }
                        }
                    }
                    while (!result.IsFinished);

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);

                    return GetFinalJobStatus(batches, result.State);
                }
            }
            catch (Exception ex)
            {
                const string errorMsg = "Error occurred in Sync Job progress monitoring.";
                _logger.LogError(ex, errorMsg);
                return ExecutionResult.Failure($"{errorMsg}. Error: {ex.Message}");
            }
        }

        private async Task<ImportDetails> GetImportStatusAsync(IImportJobController jobController, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            ValueResponse<ImportDetails> response = await jobController.GetDetailsAsync(
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.ExportRunId)
                .ConfigureAwait(false);

            return response.UnwrapOrThrow();
        }

        private async Task HandleDataSourceStatusAsync(
            List<IBatch> batches,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            using (IImportSourceController sourceController = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                foreach (IBatch batch in batches.Where(x => !x.IsFinished))
                {
                    DataSourceDetails dataSourceDetails = (await sourceController.GetDetailsAsync(
                            configuration.DestinationWorkspaceArtifactId,
                            configuration.ExportRunId,
                            batch.BatchGuid)
                        .ConfigureAwait(false))
                        .Value;

                    ImportProgress dataSourceProgress = (await sourceController.GetProgressAsync(
                            configuration.DestinationWorkspaceArtifactId,
                            configuration.ExportRunId,
                            batch.BatchGuid)
                        .ConfigureAwait(false))
                        .Value;

                    _logger.LogInformation("Data source details: {@details}", dataSourceDetails);

                    if (dataSourceDetails.IsFinished())
                    {
                        _logger.LogInformation("DataSource {dataSource} has finished with status {dataSourceState}.", batch.BatchGuid, dataSourceDetails.State);
                        await EndBatchAsync(batch, dataSourceDetails, dataSourceProgress, configuration).ConfigureAwait(false);
                    }
                }
            }
        }

        private ExecutionResult GetFinalJobStatus(List<IBatch> batches, ImportState importState)
        {
            switch (importState)
            {
                case ImportState.Failed:
                    return ExecutionResult.Failure("Error - job failed");
                case ImportState.Canceled:
                    return ExecutionResult.Canceled();
                case ImportState.Completed:
                    return batches.Any(x => x.Status == BatchStatus.CompletedWithErrors) ?
                        ExecutionResult.SuccessWithErrors() : ExecutionResult.Success();
                default:
                    const string errorMsg = "Unknown Import Job state - {importState}";
                    _logger.LogError(errorMsg, importState);
                    return ExecutionResult.Failure(string.Format(errorMsg, importState));
            }
        }

        private async Task EndBatchAsync(IBatch batch, DataSourceDetails dataSourceDetails, ImportProgress dataSourceProgress, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            switch (dataSourceDetails.State)
            {
                case DataSourceState.Completed:
                    await batch.SetStatusAsync(BatchStatus.Completed).ConfigureAwait(false);
                    break;
                case DataSourceState.Canceled:
                    await batch.SetStatusAsync(BatchStatus.Cancelled).ConfigureAwait(false);
                    break;
                case DataSourceState.CompletedWithItemErrors:
                    await HandleDataSourceErrorsAsync(
                            configuration.DestinationWorkspaceArtifactId,
                            configuration.ExportRunId,
                            batch.BatchGuid)
                        .ConfigureAwait(false);

                    await batch.SetStatusAsync(BatchStatus.CompletedWithErrors).ConfigureAwait(false);
                    break;
                case DataSourceState.Failed:
                    await batch.SetStatusAsync(BatchStatus.Failed).ConfigureAwait(false);
                    break;
                default:
                    _logger.LogWarning("Incorrect attempt of changing batch status. Batch {batchGuid} should not be marked as finished because it's current state is {state}", batch.BatchGuid, dataSourceDetails.State);
                    break;
            }

            await batch.SetTransferredItemsCountAsync(dataSourceProgress.ImportedRecords).ConfigureAwait(false);
            await batch.SetFailedItemsCountAsync(dataSourceProgress.ErroredRecords).ConfigureAwait(false);
        }

        private async Task HandleDataSourceErrorsAsync(int workspaceId, Guid importJobId, Guid dataSourceId)
        {
            int start = 0, length = 1000;
            using (IImportSourceController dataSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            {
                ImportErrors errors;
                do
                {
                    ValueResponse<ImportErrors> response = await dataSource.GetItemErrorsAsync(
                            workspaceId,
                            importJobId,
                            dataSourceId,
                            start: start,
                            length: length)
                        .ConfigureAwait(false);

                    errors = response.UnwrapOrThrow();

                    start += length;

                    await _itemLevelErrorHandler.HandleIAPIItemLevelErrorsAsync(errors).ConfigureAwait(false);
                }
                while (errors.HasMoreRecords);

                await _itemLevelErrorHandler.HandleRemainingErrorsAsync().ConfigureAwait(false);
            }
        }

        private async Task CancelImportJob(IDocumentSynchronizationMonitorConfiguration configuration, IImportJobController jobController)
        {
            _logger.LogInformation("Cancelling Import Job {jobId}...", configuration.ExportRunId);

            var status = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);
            if (status.State != ImportState.Canceled)
            {
                _logger.LogInformation("Raise cancellation request for Import Job {jobId}.", configuration.ExportRunId);
                Response response = await jobController.CancelAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId)
                    .ConfigureAwait(false);
                response.Validate();
            }
        }
    }
}
