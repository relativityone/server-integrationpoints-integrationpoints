using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DestinationWorkspaceTagsCreationNode : SyncNode<IDestinationWorkspaceTagsCreationConfiguration>
	{
		public DestinationWorkspaceTagsCreationNode(ICommand<IDestinationWorkspaceTagsCreationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating tags in destination workspace";
		}
	}
}