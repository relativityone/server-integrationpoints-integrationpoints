using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal class ImageSynchronizationNode : SyncNode<ISynchronizationConfiguration>
	{
		public ImageSynchronizationNode(ICommand<ISynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Synchronizing images";
		}
	}
}
