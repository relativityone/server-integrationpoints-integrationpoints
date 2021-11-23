using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class ObjectLinkingSnapshotPartitionNode : SyncNode<IObjectLinkingSnapshotPartitionConfiguration>
	{
		public ObjectLinkingSnapshotPartitionNode(ICommand<IObjectLinkingSnapshotPartitionConfiguration> command, ISyncLog logger)
			: base(command, logger)
		{
			Id = "Partitioning snapshot into batches for objects linking";
		}
	}
}