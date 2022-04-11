using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class NonDocumentObjectDataSourceSnapshotNode : SyncNode<INonDocumentDataSourceSnapshotConfiguration>
	{
		public NonDocumentObjectDataSourceSnapshotNode(ICommand<INonDocumentDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for non-document objects push";
		}
	}
}
