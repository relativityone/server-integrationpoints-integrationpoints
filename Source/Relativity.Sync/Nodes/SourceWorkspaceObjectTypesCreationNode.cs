using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SourceWorkspaceObjectTypesCreationNode : SyncNode<ISourceWorkspaceObjectTypesCreationConfiguration>
	{
		public SourceWorkspaceObjectTypesCreationNode(ICommand<ISourceWorkspaceObjectTypesCreationConfiguration> command, ISyncLog logger) 
			: base(command, logger)
		{
			Id = "Creating object types in source workspace";
		}
	}
}