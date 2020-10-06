using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal class ImageSynchronizationNode : SyncNode<IImageSynchronizationConfiguration>
	{
		public ImageSynchronizationNode(ICommand<IImageSynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Synchronizing images";
		}
	}
}
