using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class ImageDataSourceSnapshotNode : SyncNode<IDataSourceSnapshotConfiguration>
	{
		public ImageDataSourceSnapshotNode(ICommand<IDataSourceSnapshotConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating image data source snapshots";
		}
	}
}