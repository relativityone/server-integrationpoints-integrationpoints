using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class JobCleanupNode : SyncNode<IJobCleanupConfiguration>
	{
		public JobCleanupNode(ICommand<IJobCleanupConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Cleaning up after job";
	}
}