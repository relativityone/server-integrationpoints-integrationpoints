using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal abstract class SnapshotPartitionExecutorBase<T>: IExecutor<T> where T : ISnapshotPartitionConfiguration
    {
        private readonly SnapshotBatchHelper<T> _snapshotBatchHelper;
        private readonly ISyncLog _logger;

        protected SnapshotPartitionExecutorBase(IBatchRepository batchRepository, ISyncLog logger)
        {
            _logger = logger;
            _snapshotBatchHelper = new SnapshotBatchHelper<T>(batchRepository, logger);
        }

        public async Task<ExecutionResult> ExecuteAsync(T configuration, CompositeCancellationToken token)
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

        protected virtual void LogSnapshotPartitionsInformation(T configuration)
        {
            _logger.LogInformation(
                "Creating snapshot partitions for source workspace (workspace artifact id: {sourceWorkspaceArtifactId})",
                configuration.SourceWorkspaceArtifactId);
        }

        private async Task<IBatch> GetLastBatchAsync(T configuration)
        {
            return await _snapshotBatchHelper.GetLastBatchAsync(configuration);
        }

        private int GetNumberOfRecordsIncludedInBatches(IBatch batch)
        {
            return _snapshotBatchHelper.GetNumberOfRecordsIncludedInBatches(batch);
        }

        private async Task<ExecutionResult> CreateBatchesAsync(T configuration,
            int numberOfRecordsIncludedInBatches)
        {
            return await _snapshotBatchHelper.CreateBatchesAsync(configuration, numberOfRecordsIncludedInBatches);
        }
    }
}