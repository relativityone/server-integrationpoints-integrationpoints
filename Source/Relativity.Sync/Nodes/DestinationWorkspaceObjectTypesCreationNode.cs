using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DestinationWorkspaceObjectTypesCreationNode : SyncNode<IDestinationWorkspaceObjectTypesCreationConfiguration>
	{
		public DestinationWorkspaceObjectTypesCreationNode(ICommand<IDestinationWorkspaceObjectTypesCreationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating object types in destination workspace";
		}
	}
}