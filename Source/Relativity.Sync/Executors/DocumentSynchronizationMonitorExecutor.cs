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
using Relativity.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.Extensions;
using Relativity.Sync.KeplerFactory;
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
            ExecutionResult jobStatus;
            try
            {
                using (IImportSourceController sourceController = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
                using (IImportJobController jobController = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                using (await _progressHandler.AttachAsync(
                    configuration.SourceWorkspaceArtifactId,
                    configuration.DestinationWorkspaceArtifactId,
                    configuration.JobHistoryArtifactId,
                    configuration.ExportRunId))
                {
                    IEnumerable<IBatch> allBatchQuery = await _batchRepository.GetAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, configuration.ExportRunId).ConfigureAwait(false);
                    List<IBatch> batches = allBatchQuery.ToList();

                    TimeSpan statusCheckDelay = await _instanceSettings
                        .GetImportAPIStatusCheckDelayAsync(_DEFAULT_STATUS_CHECK_DELAY)
                        .ConfigureAwait(false);

                    ImportDetails result;
                    _logger.LogInformation("Retrieved batches to monitor: {@batches}", batches.Where(x => !x.IsFinished).Select(x => x.BatchGuid).ToList());
                    do
                    {
                        if (token.IsStopRequested) // TODO: We should not rely on BatchStatus.Cancelled
                        {
                            await CancelImportJob(batches, configuration, jobController).ConfigureAwait(false);
                        }

                        if (token.IsDrainStopRequested)
                        {
                            return ExecutionResult.Paused();
                        }

                        await Task.Delay(statusCheckDelay).ConfigureAwait(false);

                        result = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);
                        await HandleDataSourceStatusAsync(batches, sourceController, configuration).ConfigureAwait(false);
                    }
                    while (!result.IsFinished);

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);
                    jobStatus = GetFinalJobStatus(batches, result.State, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Document synchronization monitoring error");
                jobStatus = ExecutionResult.Failure($"Job progress monitoring failed. {ex.Message}");
            }

            return jobStatus;
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
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            foreach (IBatch batch in batches.Where(x => !x.IsFinished))
            {
                DataSourceDetails dataSource = (await sourceController.GetDetailsAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId,
                        batch.BatchGuid)
                    .ConfigureAwait(false))
                    .Value;
                if (dataSource.State >= DataSourceState.Canceled)
                {
                    _logger.LogInformation("DataSource {dataSource} has finished with status {state}.", batch.BatchGuid, dataSource.State);
                    await EndBatchAsync(batch, dataSource.State, configuration).ConfigureAwait(false);
                }
            }
        }

        private ExecutionResult GetFinalJobStatus(List<IBatch> batches, ImportState jobState, CompositeCancellationToken token)
        {
            if (token.IsStopRequested && jobState != ImportState.Canceled)
            {
                _logger.LogInformation("Cancellation was requested for Sync job but IAPI 2.0 returned job import state: {importState}", jobState.ToString());
            }

            switch (jobState)
            {
                case ImportState.Failed:
                    return ExecutionResult.Failure("Error - job failed");
                case ImportState.Canceled:
                    return ExecutionResult.Canceled();
                case ImportState.Paused:
                    return ExecutionResult.Paused();
                case ImportState.Completed:
                    return batches.Any(x => x.Status == BatchStatus.CompletedWithErrors) ?
                        ExecutionResult.SuccessWithErrors() : ExecutionResult.Success();
                default:
                    _logger.LogError("Unknown import job state. Received value: {importState}", jobState);
                    return ExecutionResult.Failure("Unknown job import state");
            }
        }

        private async Task EndBatchAsync(IBatch batch, DataSourceState state, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            switch (state)
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
                    _logger.LogWarning("Incorrect attempt of changing batch status. Batch {batchGuid} should not be marked as finished because it's current state is {state}", batch.BatchGuid, state);
                    break;
            }
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
            }
        }

        private async Task CancelImportJob(List<IBatch> batches, IDocumentSynchronizationMonitorConfiguration configuration, IImportJobController jobController)
        {
            var status = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);
            if (status.State != ImportState.Canceled)
            {
                _logger.LogInformation("Executing job {jobId} cancel request at monitoring stage", configuration.ExportRunId);
                Response response = await jobController.CancelAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);
                response.Validate();
            }
        }
    }
}
