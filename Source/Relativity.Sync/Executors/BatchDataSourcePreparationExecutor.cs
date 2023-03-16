using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Progress;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal class BatchDataSourcePreparationExecutor : IExecutor<IBatchDataSourcePreparationConfiguration>
    {
        private readonly IImportService _importService;
        private readonly IBatchRepository _batchRepository;
        private readonly ILoadFileGenerator _fileGenerator;
        private readonly IProgressHandler _progressHandler;
        private readonly IAPILog _logger;

        public BatchDataSourcePreparationExecutor(
            IImportService importService,
            IBatchRepository batchRepository,
            ILoadFileGenerator fileGenerator,
            IProgressHandler progressHandler,
            IAPILog logger)
        {
            _importService = importService;
            _batchRepository = batchRepository;
            _fileGenerator = fileGenerator;
            _progressHandler = progressHandler;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CompositeCancellationToken token)
        {
            List<int> batchIdList = (await _batchRepository.GetAllBatchesIdsToExecuteAsync(
                                    configuration.SourceWorkspaceArtifactId,
                                    configuration.SyncConfigurationArtifactId,
                                    configuration.ExportRunId)
                    .ConfigureAwait(false))
                    .ToList();

            _logger.LogInformation("Retrieved {batchesCount} Batches to Import.", batchIdList.Count);

            using (await _progressHandler.AttachAsync(
                           configuration.SourceWorkspaceArtifactId,
                           configuration.DestinationWorkspaceArtifactId,
                           configuration.JobHistoryArtifactId,
                           configuration.ExportRunId,
                           configuration.SyncConfigurationArtifactId,
                           batchIdList)
                       .ConfigureAwait(false))
            {
                try
                {
                    foreach (int batchId in batchIdList)
                    {
                        _logger.LogInformation("Reading Batch {batchId}...", batchId);
                        IBatch batch = await _batchRepository.GetAsync(configuration.SourceWorkspaceArtifactId, batchId).ConfigureAwait(false);
                        ILoadFile loadFile = await _fileGenerator.GenerateAsync(batch, token).ConfigureAwait(false);

                        if (batch.Status == BatchStatus.Cancelled)
                        {
                            await _importService.CancelJobAsync().ConfigureAwait(false);

                            // After every cancellation we need to end this execution with success to get into DocumentSynchronizationMonitorExecutor
                            return ExecutionResult.Success();
                        }

                        if (batch.Status == BatchStatus.Paused)
                        {
                            return ExecutionResult.Paused();
                        }

                        try
                        {
                            await _importService.AddDataSourceAsync(batch.BatchGuid, loadFile.Settings).ConfigureAwait(false);
                        }
                        catch (SyncException)
                        {
                            await _importService.CancelJobAsync().ConfigureAwait(false);
                            await batch.SetStatusAsync(BatchStatus.Failed).ConfigureAwait(false);
                            return ExecutionResult.Success();
                        }

                        await batch.SetStatusAsync(BatchStatus.Generated).ConfigureAwait(false);
                        await _progressHandler.HandleProgressAsync().ConfigureAwait(false);
                    }

                    await _progressHandler.HandleProgressAsync().ConfigureAwait(false);
                    await _importService.EndJobAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch Data Source preparation failed");
                    await _importService.CancelJobAsync().ConfigureAwait(false);
                }
            }

            return ExecutionResult.Success();
        }
    }
}
