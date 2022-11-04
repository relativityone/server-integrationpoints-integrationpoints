using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Import.V1;
using Relativity.Import.V1.Services;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal class BatchDataSourcePreparationExecutor : IExecutor<IBatchDataSourcePreparationConfiguration>
    {
        private readonly IDestinationServiceFactoryForUser _serviceFactory;
        private readonly IBatchRepository _batchRepository;
        private readonly ILoadFileGenerator _fileGenerator;
        private readonly IAPILog _logger;

        public BatchDataSourcePreparationExecutor(
            IDestinationServiceFactoryForUser serviceFactory,
            IBatchRepository batchRepository,
            ILoadFileGenerator fileGenerator,
            IAPILog logger)
        {
            _serviceFactory = serviceFactory;
            _batchRepository = batchRepository;
            _fileGenerator = fileGenerator;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CompositeCancellationToken token)
        {
            using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
            using (IImportJobController job = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
            {
                try
                {
                    List<int> batchIdList = (await _batchRepository.GetAllBatchesIdsToExecuteAsync(
                                             configuration.SourceWorkspaceArtifactId,
                                             configuration.SyncConfigurationArtifactId,
                                             configuration.ExportRunId)
                             .ConfigureAwait(false))
                             .ToList();

                    foreach (int batchId in batchIdList)
                    {
                        IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
                        ILoadFile loadFile = await _fileGenerator.GenerateAsync(batch, token).ConfigureAwait(false);

                        if (token.IsStopRequested)
                        {
                            await CancelJobAsync(job, configuration).ConfigureAwait(false);
                            return ExecutionResult.Canceled();
                        }

                        if (token.IsDrainStopRequested)
                        {
                            return ExecutionResult.Paused();
                        }

                        Response result = await importSource.AddSourceAsync(
                             configuration.DestinationWorkspaceArtifactId,
                             configuration.ExportRunId,
                             batch.BatchGuid,
                             loadFile.Settings)
                             .ConfigureAwait(false);

                        if (!result.IsSuccess)
                        {
                            await CancelJobAsync(job, configuration).ConfigureAwait(false);
                            return ExecutionResult.Failure($"Could not send load file for batch ID: {batch.ArtifactId} to IAPI 2.0. {result.ErrorMessage}");
                        }

                        await batch.SetStatusAsync(BatchStatus.Generated).ConfigureAwait(false);
                    }

                    await EndJobAsync(job, configuration).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch Data Source preparation failed");
                    await CancelJobAsync(job, configuration).ConfigureAwait(false);
                    return ExecutionResult.Failure(ex);
                }
            }

            return ExecutionResult.Success();
        }

        private async Task CancelJobAsync(IImportJobController job, IBatchDataSourcePreparationConfiguration configuration)
        {
            Response result = await job.CancelAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                _logger.LogError("Could not cancel Job ID = {jobId}. {errorMessage}", configuration.ExportRunId, result.ErrorMessage);
            }
        }

        private async Task EndJobAsync(IImportJobController job, IBatchDataSourcePreparationConfiguration configuration)
        {
            Response result = await job.EndAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                _logger.LogError("Could not end Job ID = {jobId}. {errorMessage}", configuration.ExportRunId, result.ErrorMessage);
            }
        }
    }
}
