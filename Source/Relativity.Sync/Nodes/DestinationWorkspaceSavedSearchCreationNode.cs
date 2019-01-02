using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DestinationWorkspaceSavedSearchCreationNode : SyncNode<IDestinationWorkspaceSavedSearchCreationConfiguration>
	{
		public DestinationWorkspaceSavedSearchCreationNode(ICommand<IDestinationWorkspaceSavedSearchCreationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Creating saved search in destination workspace";
		}
	}
}