using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal abstract class SnapshotPartitionExecutorBase: IExecutor<ISnapshotPartitionConfiguration>
    {
        private readonly ISyncLog _logger;
        private readonly IBatchRepository _batchRepository;

        protected SnapshotPartitionExecutorBase(IBatchRepository batchRepository, ISyncLog logger)
        {
            _batchRepository = batchRepository;
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteAsync(ISnapshotPartitionConfiguration configuration, CompositeCancellationToken token)
        {
            LogSnapshotPartitionsInformation(configuration);
            IBatch batch;
            try
            {
                batch = await GetLastBatchAsync(configuration).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return ExecutionResult.Failure("Cannot read last batch.", e);
            }

            int numberOfRecordsIncludedInBatches = GetNumberOfRecordsIncludedInBatches(batch);
            ExecutionResult executionResult = await CreateBatchesAsync(configuration, numberOfRecordsIncludedInBatches).ConfigureAwait(false);

            return executionResult;
        }

        protected virtual void LogSnapshotPartitionsInformation(ISnapshotPartitionConfiguration configuration)
        {
            _logger.LogInformation(
                "Creating snapshot partitions for source workspace (workspace artifact id: {sourceWorkspaceArtifactId})",
                configuration.SourceWorkspaceArtifactId);
        }

        private async Task<IBatch> GetLastBatchAsync(ISnapshotPartitionConfiguration configuration)
        {
            IBatch batch;
            try
            {
                batch = await _batchRepository.GetLastAsync(configuration.SourceWorkspaceArtifactId,
                    configuration.SyncConfigurationArtifactId, configuration.ExportRunId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to retrieve last batch for sync configuration {artifactId}.",
                    configuration.SyncConfigurationArtifactId);
                throw;
            }

            return batch;
        }

        private int GetNumberOfRecordsIncludedInBatches(IBatch batch)
        {
            int numberOfRecordsIncludedInBatches = 0;
            if (batch != null)
            {
                numberOfRecordsIncludedInBatches = batch.StartingIndex + batch.TotalDocumentsCount;
                _logger.LogInformation("Last batch was not null. Starting partitioning at index {index}",
                    numberOfRecordsIncludedInBatches);
            }
            else
            {
                _logger.LogInformation("Partitioning from start");
            }

            return numberOfRecordsIncludedInBatches;
        }

        private async Task<ExecutionResult> CreateBatchesAsync(ISnapshotPartitionConfiguration configuration,
            int numberOfRecordsIncludedInBatches)
        {
            Snapshot snapshot = new Snapshot(configuration.TotalRecordsCount, configuration.BatchSize,
                numberOfRecordsIncludedInBatches);

            try
            {
                foreach (SnapshotPart snapshotPart in snapshot.GetSnapshotParts())
                {
                    await _batchRepository.CreateAsync(configuration.SourceWorkspaceArtifactId,
                            configuration.SyncConfigurationArtifactId, configuration.ExportRunId, snapshotPart.NumberOfRecords,
                            snapshotPart.StartingIndex)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to create batch for sync configuration {artifactId}.",
                    configuration.SyncConfigurationArtifactId);
                return ExecutionResult.Failure("Unable to create batches.", e);
            }

            return ExecutionResult.Success();
        }
    }
}