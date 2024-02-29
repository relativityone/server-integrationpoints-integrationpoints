using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class RetryDataSourceSnapshotNode : SyncNode<IRetryDataSourceSnapshotConfiguration>
    {
        public RetryDataSourceSnapshotNode(ICommand<IRetryDataSourceSnapshotConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Creating data source snapshots for documents with errors";
        }
    }
}
