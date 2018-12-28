using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
	internal sealed class JobStatusConsolidationNode : SyncNode<IJobStatusConsolidationConfiguration>
	{
		public JobStatusConsolidationNode(ICommand<IJobStatusConsolidationConfiguration> command, ISyncLog logger) : base(command, logger)
		{
		}

		protected override string Name => "Consolidating job status";
	}
}