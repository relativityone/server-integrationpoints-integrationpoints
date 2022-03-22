using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.SumReporting
{
	internal class NonDocumentJobStartMetricsNode : SyncNode<INonDocumentJobStartMetricsConfiguration>
	{
		public NonDocumentJobStartMetricsNode(ICommand<INonDocumentJobStartMetricsConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Reporting non-document job start metrics";
		}
	}
}
