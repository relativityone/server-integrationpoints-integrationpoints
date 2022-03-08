using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class SnapshotPartitionExecutor : SnapshotPartitionExecutorBase 
	{
		public SnapshotPartitionExecutor(IBatchRepository batchRepository, ISyncLog logger)
		    : base(batchRepository, logger)
		{
		}
	}
}