using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Models;
using Relativity.Import.V1.Models.Sources;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationMonitorExecutor : IExecutor<IDocumentSynchronizationMonitorConfiguration>
    {
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IProgressHandler _progressHandler;
        private readonly IBatchRepository _batchRepository;
        private readonly IAPILog _logger;

        public DocumentSynchronizationMonitorExecutor(
            IDestinationServiceFactoryForUser serviceFactory,
            IProgressHandler progressHandler,
            IBatchRepository batchRepository,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _progressHandler = progressHandler;
            _batchRepository = batchRepository;
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
                    List<IBatch> batches = _batchRepository.GetAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, configuration.ExportRunId)
                                                              .GetAwaiter()
                                                              .GetResult()
                                                              .ToList();

                    _logger.LogInformation("Retrieved DataSources to monitor: {@dataSources}", batches.Where(x => !x.IsFinished).Select(x => x.BatchGuid).ToList());

                    ValueResponse<ImportDetails> result = null;
                    do
                    {
                        if (token.IsStopRequested)
                        {
                            await HandleJobCancellation(batches, configuration, jobController).ConfigureAwait(false);
                        }

                        if (token.IsDrainStopRequested)
                        {
                            return ExecutionResult.Paused();
                        }

                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                        result = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);
                        await HandleDataSourceStatusAsync(batches, sourceController, configuration).ConfigureAwait(false);
                    }
                    while (!result.Value.IsFinished);

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);
                    jobStatus = GetFinalJobStatus(result.Value.State, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Document synchronization monitoring error");
                jobStatus = ExecutionResult.Failure($"Job progress monitoring failed. {ex.Message}");
            }

            return jobStatus;
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

        private async Task HandleDataSourceStatusAsync(
            List<IBatch> batches,
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {

            List<IBatch> unprocessedBatches = batches.Where(x => !x.IsFinished).ToList();
            foreach (IBatch batch in unprocessedBatches)
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
                    await MarkBatchAsProcessed(batch, dataSource.State).ConfigureAwait(false);
                }
            }
        }

        private async Task<ValueResponse<ImportDetails>> GetImportStatusAsync(IImportJobController jobController, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            ValueResponse<ImportDetails> response = await jobController.GetDetailsAsync(
                                configuration.DestinationWorkspaceArtifactId,
                                configuration.ExportRunId)
                            .ConfigureAwait(false);

            return TryGetValueResponse(response);
        }

        private ValueResponse<T> TryGetValueResponse<T>(ValueResponse<T> response)
        {
            if (!response.IsSuccess)
            {
                string message = $"Error code: {response.ErrorCode}, message: {response.ErrorMessage}";
                throw new SyncException(message);
            }

            return response;
        }

        private async Task MarkBatchAsProcessed(IBatch batch, DataSourceState state)
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

        private async Task HandleJobCancellation(List<IBatch> batches, IDocumentSynchronizationMonitorConfiguration configuration, IImportJobController jobController)
        {
            if (batches.Any(x => x.Status == BatchStatus.Cancelled))
            {
                _logger.LogInformation("Waiting on cancellation of job {jobId} in IAPI 2.0", configuration.ExportRunId);
            }
            else
            {
                _logger.LogInformation("Executing job {jobId} cancel request at monitoring stage", configuration.ExportRunId);
                Response result = await jobController.CancelAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    _logger.LogError("Could not cancel Job ID = {jobId}. {errorMessage}", configuration.ExportRunId, result.ErrorMessage);
                }
            }
        }
    }
}
