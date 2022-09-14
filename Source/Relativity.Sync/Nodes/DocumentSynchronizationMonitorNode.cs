using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class DocumentSynchronizationMonitorNode : SyncNode<IDocumentSynchronizationMonitorConfiguration>
    {
        public DocumentSynchronizationMonitorNode(ICommand<IDocumentSynchronizationMonitorConfiguration> command, IAPILog logger)
            : base(command, logger)
        {
        }
    }
}
