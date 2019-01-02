using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class PreviousRunCleanupNode : SyncNode<IPreviousRunCleanupConfiguration>
	{
		public PreviousRunCleanupNode(ICommand<IPreviousRunCleanupConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Cleaning up after last run";
		}
	}
}