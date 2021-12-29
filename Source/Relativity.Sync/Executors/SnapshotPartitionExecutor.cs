using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class SnapshotPartitionExecutor : SnapshotPartitionExecutorBase<ISnapshotPartitionConfiguration> 
	{
		public SnapshotPartitionExecutor(IBatchRepository batchRepository, ISyncLog logger)
		    : base(batchRepository, logger)
		{
		}
	}
}