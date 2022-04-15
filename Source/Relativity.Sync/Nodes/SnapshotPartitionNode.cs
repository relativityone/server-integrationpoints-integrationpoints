using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SnapshotPartitionNode : SyncNode<ISnapshotPartitionConfiguration>
	{
		public SnapshotPartitionNode(ICommand<ISnapshotPartitionConfiguration> command, IAPILog logger) : base(command, logger)
		{
			Id = "Partitioning snapshot into batches";
		}
	}
}
