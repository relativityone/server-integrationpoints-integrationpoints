using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentDataSourceSnapshotNode : SyncNode<IDocumentDataSourceSnapshotConfiguration>
	{
		public DocumentDataSourceSnapshotNode(ICommand<IDocumentDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents push";
		}
	}
}