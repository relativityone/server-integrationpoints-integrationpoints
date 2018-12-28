using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DestinationWorkspaceTagsCreationNode : SyncNode<IDestinationWorkspaceTagsCreationConfiguration>
	{
		public DestinationWorkspaceTagsCreationNode(ICommand<IDestinationWorkspaceTagsCreationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Creating tags in destination workspace";
	}
}