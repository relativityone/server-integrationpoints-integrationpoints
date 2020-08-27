using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class ImageRetryDataSourceSnapshotNode : SyncNode<IRetryDataSourceSnapshotConfiguration>
	{
		public ImageRetryDataSourceSnapshotNode(ICommand<IRetryDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating image data source snapshots for images with errors";
		}
	}
}
