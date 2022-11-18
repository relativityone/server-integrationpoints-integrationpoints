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

        private List<IBatch> _batches;

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
            // Followig if statements are temporary solution. Logic for handling Drain Stop and Cancel tokens should be implemented within: REL-770973
            if (token.IsDrainStopRequested)
            {
                return ExecutionResult.Paused();
            }

            if (token.IsStopRequested)
            {
                return ExecutionResult.Canceled();
            }

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
                    _batches = GetUnprocessedBatchesForJob(configuration);

                    List<Guid> dataSources = await GetDataSourcesAsync(jobController, configuration).ConfigureAwait(false);

                    _logger.LogInformation("Retrieved DataSources to monitor: {@dataSources}", dataSources);

                    ValueResponse<ImportDetails> result = null;
                    do
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                        result = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);

                        await HandleDataSourceStatusAsync(dataSources, sourceController, configuration).ConfigureAwait(false);
                    }
                    while (!result.Value.IsFinished);

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);

                    jobStatus = GetFinalJobStatus(result.Value.State);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Document synchronization monitoring error");
                jobStatus = ExecutionResult.Failure($"Job progress monitoring failed. {ex.Message}");
            }

            return jobStatus;
        }

        private ExecutionResult GetFinalJobStatus(ImportState jobState)
        {
            switch (jobState)
            {
                case ImportState.Failed:
                    return ExecutionResult.Failure("Error - job failed");
                case ImportState.Canceled:
                    return ExecutionResult.Canceled();
                case ImportState.Paused:
                    return ExecutionResult.Paused();
                case ImportState.Completed:
                    return _batches.Any(x => x.Status == BatchStatus.CompletedWithErrors) ?
                        ExecutionResult.SuccessWithErrors() : ExecutionResult.Success();
                default:
                    _logger.LogError("Unknown import job state. Received value: {importState}", jobState);
                    return ExecutionResult.Failure("Unknown job import state");
            }
        }

        private async Task HandleDataSourceStatusAsync(
            List<Guid> dataSources,
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            foreach (Guid sourceId in dataSources.Except(_batches.Where(x => x.IsCompleted).Select(x => x.BatchGuid)))
            {
                DataSourceDetails dataSource = (await sourceController.GetDetailsAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId,
                        sourceId)
                    .ConfigureAwait(false))
                    .Value;

                if (dataSource.State >= DataSourceState.Canceled)
                {
                    _logger.LogInformation("DataSource {dataSource} has finished with status {state}.", sourceId, dataSource.State);
                    await MarkBatchAsProcessed(sourceId, dataSource.State).ConfigureAwait(false);
                }
            }
        }

        private async Task<List<Guid>> GetDataSourcesAsync(IImportJobController jobController, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            ValueResponse<DataSources> response = await jobController.GetSourcesAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId)
                    .ConfigureAwait(false);

            DataSources allDataSources = TryGetValueResponse(response).Value;

            IEnumerable<Guid> dataSourcesWithUnfinishedState = from dataSource in allDataSources.Sources
                                                               join unprocessedBatch in _batches
                                                               on dataSource equals unprocessedBatch.BatchGuid
                                                               select dataSource;
            return dataSourcesWithUnfinishedState.ToList();
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

        private List<IBatch> GetUnprocessedBatchesForJob(IDocumentSynchronizationMonitorConfiguration configuration)
        {
            List<IBatch> allBatches = _batchRepository.GetAllAsync(configuration.SourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, configuration.ExportRunId)
                 .GetAwaiter()
                 .GetResult()
                 .ToList();

            return allBatches.Where(x => !x.IsCompleted)
                            .ToList();
        }

        private async Task MarkBatchAsProcessed(Guid batchGuid, DataSourceState state)
        {
            IBatch batch = _batches.Where(x => x.BatchGuid == batchGuid).Single();

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
    }
}
