using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DataSourceSnapshotNode : SyncNode<IDataSourceSnapshotConfiguration>
	{
		public DataSourceSnapshotNode(ICommand<IDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents push";
		}
	}
}