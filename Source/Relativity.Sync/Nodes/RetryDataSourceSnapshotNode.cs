using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class RetryDataSourceSnapshotNode : SyncNode<IRetryDataSourceSnapshotConfiguration>
	{
		public RetryDataSourceSnapshotNode(ICommand<IRetryDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for documents with errors";
		}
	}
}
