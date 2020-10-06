using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentSynchronizationNode : SyncNode<IDocumentSynchronizationConfiguration>
	{
		public DocumentSynchronizationNode(ICommand<IDocumentSynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Synchronizing documents";
		}
	}
}