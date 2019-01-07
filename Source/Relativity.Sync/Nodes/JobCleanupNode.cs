using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class JobCleanupNode : SyncNode<IJobCleanupConfiguration>
	{
		public JobCleanupNode(ICommand<IJobCleanupConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Cleaning up after job";
		}
	}
}