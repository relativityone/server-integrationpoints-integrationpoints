using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class NonDocumentObjectLinkingSynchronizationNode : SyncNode<INonDocumentObjectLinkingConfiguration>
    {
        public NonDocumentObjectLinkingSynchronizationNode(ICommand<INonDocumentObjectLinkingConfiguration> command, ISyncLog logger) : base(command, logger)
        {
            Id = "Linking Non Documents objects";
        }
    }
}