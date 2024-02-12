using Relativity.API;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
    internal sealed class SnapshotPartitionExecutor : SnapshotPartitionExecutorBase
    {
        public SnapshotPartitionExecutor(IBatchRepository batchRepository, IAPILog logger)
            : base(batchRepository, logger)
        {
        }
    }
}
