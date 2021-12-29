using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal class SnapshotBatchHelper<T> where T : ISnapshotPartitionConfiguration
    {
        private readonly IBatchRepository _batchRepository;
        private readonly ISyncLog _logger;

        internal SnapshotBatchHelper(IBatchRepository batchRepository, ISyncLog logger)
        {
            _batchRepository = batchRepository;
            _logger = logger;
        }

        internal async Task<IBatch> GetLastBatchAsync(T configuration)
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

        internal int GetNumberOfRecordsIncludedInBatches(IBatch batch)
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

        public async Task<ExecutionResult> CreateBatchesAsync(T configuration,
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