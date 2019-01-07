using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SnapshotPartitionNode : SyncNode<ISnapshotPartitionConfiguration>
	{
		public SnapshotPartitionNode(ICommand<ISnapshotPartitionConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Partitioning snapshot into batches";
		}
	}
}