using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SnapshotPartitionNode : SyncNode<ISnapshotPartitionConfiguration>
	{
		public SnapshotPartitionNode(ICommand<ISnapshotPartitionConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Partitioning snapshot into batches";
	}
}