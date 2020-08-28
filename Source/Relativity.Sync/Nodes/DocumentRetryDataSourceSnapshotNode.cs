using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentRetryDataSourceSnapshotNode : SyncNode<IDocumentRetryDataSourceSnapshotConfiguration>
	{
		public DocumentRetryDataSourceSnapshotNode(ICommand<IDocumentRetryDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents with errors";
		}
	}
}
