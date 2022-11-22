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
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationMonitorExecutor : IExecutor<IDocumentSynchronizationMonitorConfiguration>
    {
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IProgressHandler _progressHandler;
        private readonly IAPILog _logger;
        private readonly IItemLevelErrorHandlerFactory _itemLevelErrorHandlerFactory;

        public DocumentSynchronizationMonitorExecutor(
            IDestinationServiceFactoryForUser serviceFactory,
            IProgressHandler progressHandler,
            IItemLevelErrorHandlerFactory itemLevelErrorHandlerFactory,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _progressHandler = progressHandler;
            _itemLevelErrorHandlerFactory = itemLevelErrorHandlerFactory;
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
                    DataSources dataSources = await GetDataSourcesAsync(jobController, configuration).ConfigureAwait(false);

                    _logger.LogInformation("Retrieved DataSources to monitor: {@dataSources}", dataSources.Sources);

                    IDictionary<Guid, DataSourceState> processedSources = new Dictionary<Guid, DataSourceState>();
                    ValueResponse<ImportDetails> result;
                    do
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                        result = await GetImportStatusAsync(jobController, configuration).ConfigureAwait(false);

                        await HandleDataSourceStatusAsync(dataSources, processedSources, sourceController, configuration).ConfigureAwait(false);
                    }
                    while (!result.Value.IsFinished);

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);

                    jobStatus = GetFinalJobStatus(result.Value.State, processedSources);

                    IItemLevelErrorHandler itemLevelErrorHandler = _itemLevelErrorHandlerFactory.Create(new ItemStatusMonitor());

                    await itemLevelErrorHandler.HandleIApiItemLevelErrors(sourceController, dataSources, configuration)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Document synchronization monitoring error");
                jobStatus = ExecutionResult.Failure($"Job progress monitoring failed. {ex.Message}");
            }


            return jobStatus;
        }

        private ExecutionResult GetFinalJobStatus(ImportState jobState, IDictionary<Guid, DataSourceState> processedSources)
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
                    return processedSources.Any(x => x.Value == DataSourceState.CompletedWithItemErrors) ?
                        ExecutionResult.SuccessWithErrors() : ExecutionResult.Success();
                default:
                    _logger.LogError("Unknown import job state. Received value: {importState}", jobState);
                    return ExecutionResult.Failure("Unknown job import state");
            }
        }

        private async Task HandleDataSourceStatusAsync(
            DataSources dataSources,
            IDictionary<Guid, DataSourceState> processedSources,
            IImportSourceController sourceController,
            IDocumentSynchronizationMonitorConfiguration configuration)
        {
            foreach (Guid sourceId in dataSources.Sources.Except(processedSources.Keys))
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

                    processedSources.Add(sourceId, dataSource.State);
                }
            }
        }

        private async Task<DataSources> GetDataSourcesAsync(IImportJobController jobController, IDocumentSynchronizationMonitorConfiguration configuration)
        {
            ValueResponse<DataSources> response = await jobController.GetSourcesAsync(
                        configuration.DestinationWorkspaceArtifactId,
                        configuration.ExportRunId)
                    .ConfigureAwait(false);

            return TryGetValueResponse(response).Value;
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
    }
}
