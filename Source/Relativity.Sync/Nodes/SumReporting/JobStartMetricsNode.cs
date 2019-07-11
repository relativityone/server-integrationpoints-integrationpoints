using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.SumReporting
{
	internal sealed class JobStartMetricsNode : SyncNode<ISumReporterConfiguration>
	{
		public JobStartMetricsNode(ICommand<ISumReporterConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Reporting job start metrics";
			ParallelGroupName = "Parallel";
		}
	}
}