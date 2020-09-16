using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class ImageDataSourceSnapshotNode : SyncNode<IImageDataSourceSnapshotConfiguration>
	{
		public ImageDataSourceSnapshotNode(ICommand<IImageDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating data source snapshots for images push";
		}
	}
}