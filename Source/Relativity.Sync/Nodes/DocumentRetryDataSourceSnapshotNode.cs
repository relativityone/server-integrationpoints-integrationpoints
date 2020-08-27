using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentRetryDataSourceSnapshotNode : SyncNode<IRetryDataSourceSnapshotConfiguration>
	{
		public DocumentRetryDataSourceSnapshotNode(ICommand<IRetryDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents with errors";
		}
	}
}
