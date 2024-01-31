using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes.SumReporting
{
    internal class DocumentJobStartMetricsNode : SyncNode<IDocumentJobStartMetricsConfiguration>
    {
        public DocumentJobStartMetricsNode(ICommand<IDocumentJobStartMetricsConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Reporting document job start metrics";
            ParallelGroupName = "Multi node";
        }
    }
}