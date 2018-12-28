using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SourceWorkspaceTagsCreationNode : SyncNode<ISourceWorkspaceTagsCreationConfiguration>
	{
		public SourceWorkspaceTagsCreationNode(ICommand<ISourceWorkspaceTagsCreationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Creating tags in source workspace";
	}
}