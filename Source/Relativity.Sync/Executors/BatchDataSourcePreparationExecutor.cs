using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public BatchDataSourcePreparationExecutor(
            IDestinationServiceFactoryForUser serviceFactory,
            IBatchRepository batchRepository,
            ILoadFileGenerator fileGenerator)
        {
            _serviceFactory = serviceFactory;
            _batchRepository = batchRepository;
            _fileGenerator = fileGenerator;
        }

        public async Task<ExecutionResult> ExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CompositeCancellationToken token)
        {
            List<int> batchIdList = (await _batchRepository
            .GetAllBatchesIdsToExecuteAsync(
                configuration.SourceWorkspaceArtifactId,
                configuration.SyncConfigurationArtifactId,
                configuration.ExportRunId)
            .ConfigureAwait(false))
            .ToList();

            try
            {
                using (IImportSourceController importSource = await _serviceFactory.CreateProxyAsync<IImportSourceController>().ConfigureAwait(false))
                {
                    using (IImportJobController job = await _serviceFactory.CreateProxyAsync<IImportJobController>().ConfigureAwait(false))
                    {
                        foreach (int batchId in batchIdList)
                        {
                            IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
                            ILoadFile loadFile = await _fileGenerator.Generate(batch).ConfigureAwait(false);

                            if (loadFile != null)
                            {
                                await importSource.AddSourceAsync(
                                    configuration.DestinationWorkspaceArtifactId,
                                    configuration.ExportRunId,
                                    batch.BatchGuid,
                                    loadFile.Settings)
                                    .ConfigureAwait(false);

                                await batch.SetStatusAsync(BatchStatus.Generated).ConfigureAwait(false);
                            }
                        }

                        Response response = await job.EndAsync(configuration.DestinationWorkspaceArtifactId, configuration.ExportRunId).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                return ExecutionResult.Failure(ex);
            }

            return ExecutionResult.Success();
        }
    }
}
