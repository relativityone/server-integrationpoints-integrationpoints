using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal class ImageSynchronizationNode : SyncNode<IImageSynchronizationConfiguration>
    {
        public ImageSynchronizationNode(ICommand<IImageSynchronizationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Synchronizing images";
        }
    }
}
