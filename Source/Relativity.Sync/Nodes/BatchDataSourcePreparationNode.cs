using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class BatchDataSourcePreparationNode : SyncNode<IBatchDataSourcePreparationConfiguration>
    {
        public BatchDataSourcePreparationNode(ICommand<IBatchDataSourcePreparationConfiguration> command, IAPILog logger)
            : base(command, logger)
        {
        }
    }
}
