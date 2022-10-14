using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class ConfigureDocumentSynchronizationNode : SyncNode<IConfigureDocumentSynchronizationConfiguration>
    {
        public ConfigureDocumentSynchronizationNode(ICommand<IConfigureDocumentSynchronizationConfiguration> command, IAPILog logger)
            : base(command, logger)
        {
            Id = "Configuring Document Synchronization for IAPI2 pipeline";
        }
    }
}
