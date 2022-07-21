using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class DestinationWorkspaceObjectTypesCreationNode : SyncNode<IDestinationWorkspaceObjectTypesCreationConfiguration>
    {
        public DestinationWorkspaceObjectTypesCreationNode(ICommand<IDestinationWorkspaceObjectTypesCreationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Creating object types in destination workspace";
        }
    }
}
