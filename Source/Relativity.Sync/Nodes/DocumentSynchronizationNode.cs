using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class DocumentSynchronizationNode : SyncNode<ISynchronizationConfiguration>
	{
		public DocumentSynchronizationNode(ICommand<ISynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Synchronizing documents";
		}
	}
}