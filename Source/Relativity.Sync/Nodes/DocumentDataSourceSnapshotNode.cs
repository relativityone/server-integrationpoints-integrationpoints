using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentDataSourceSnapshotNode : SyncNode<IDataSourceSnapshotConfiguration>
	{
		public DocumentDataSourceSnapshotNode(ICommand<IDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents push";
		}
	}
}