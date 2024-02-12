using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class NonDocumentSynchronizationNode : SyncNode<INonDocumentSynchronizationConfiguration>
    {
        public NonDocumentSynchronizationNode(ICommand<INonDocumentSynchronizationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Synchronizing Non Documents objects";
        }
    }
}
