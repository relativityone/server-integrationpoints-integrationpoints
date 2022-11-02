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

                    _logger.LogInformation("Retrieved {batchesCount} Batches to Import.", batchIdList.Count);
                    foreach (int batchId in batchIdList)
                    {
                        _logger.LogInformation("Reading Batch {batchId}...", batchId);
                        IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
                        ILoadFile loadFile = await _fileGenerator.GenerateAsync(batch).ConfigureAwait(false);

                        Response result = await importSource.AddSourceAsync(
                             configuration.DestinationWorkspaceArtifactId,
                             configuration.ExportRunId,
                             batch.BatchGuid,
                             loadFile.Settings)
                             .ConfigureAwait(false);

                        if (!result.IsSuccess)
                        {
                            await CancelJobOnFailure(job, configuration).ConfigureAwait(false);
                            return ExecutionResult.Failure($"Could not send load file for batch ID: {batch.ArtifactId} to IAPI 2.0. {result.ErrorMessage}");
                        }

                        await batch.SetStatusAsync(BatchStatus.Generated).ConfigureAwait(false);
                    }

                    Response response = await job.EndAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch Data Source preparation failed");
                    await CancelJobOnFailure(job, configuration).ConfigureAwait(false);
                    return ExecutionResult.Failure(ex);
                }
            }

            return ExecutionResult.Success();
        }

        private async Task CancelJobOnFailure(IImportJobController job, IBatchDataSourcePreparationConfiguration configuration)
        {
            Response result = await job.CancelAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);

            if (result == null || !result.IsSuccess)
            {
                _logger.LogError("Could not cancel Job. {errorMessage}", result?.ErrorMessage);
            }
        }
    }
}
