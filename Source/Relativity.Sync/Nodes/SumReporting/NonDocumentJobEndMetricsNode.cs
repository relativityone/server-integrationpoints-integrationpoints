using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.SumReporting
{
	internal class NonDocumentJobEndMetricsNode : SyncNode<INonDocumentJobEndMetricsConfiguration>
	{
		public NonDocumentJobEndMetricsNode(ICommand<INonDocumentJobEndMetricsConfiguration> command, ISyncLog logger) : base(command, logger)
		{
			Id = "Reporting non-document job end metrics";
		}
	}
}
