using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class NonDocumentSynchronizationNode : SyncNode<INonDocumentSynchronizationConfiguration>
    {
        public NonDocumentSynchronizationNode(ICommand<INonDocumentSynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
        {
            Id = "Synchronizing Non Documents objects";
        }
    }
}
