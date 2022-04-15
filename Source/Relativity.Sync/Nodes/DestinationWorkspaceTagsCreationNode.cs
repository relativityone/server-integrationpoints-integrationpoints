using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DestinationWorkspaceTagsCreationNode : SyncNode<IDestinationWorkspaceTagsCreationConfiguration>
	{
		public DestinationWorkspaceTagsCreationNode(ICommand<IDestinationWorkspaceTagsCreationConfiguration> command, IAPILog logger) : base(command, logger)
		{
			Id = "Creating tags in destination workspace";
			ParallelGroupName = "Multi node";
		}
	}
}
