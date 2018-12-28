using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DataSourceSnapshotNode : SyncNode<IDataSourceSnapshotConfiguration>
	{
		public DataSourceSnapshotNode(ICommand<IDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Creating data source snapshots";
	}
}