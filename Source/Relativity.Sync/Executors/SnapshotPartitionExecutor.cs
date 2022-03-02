using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Executors
{
	internal sealed class SnapshotPartitionExecutor : SnapshotPartitionExecutorBase 
	{
		public SnapshotPartitionExecutor(IBatchRepository batchRepository, IInstanceSettings instanceSettings, ISyncLog logger)
		    : base(batchRepository, instanceSettings, logger)
		{
		}
	}
}