using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class SynchronizationNode : SyncNode<ISynchronizationConfiguration>
	{
		public SynchronizationNode(ICommand<ISynchronizationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Synchronizing";
	}
}