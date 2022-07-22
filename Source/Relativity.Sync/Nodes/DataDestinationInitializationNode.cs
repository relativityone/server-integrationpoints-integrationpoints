using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class DataDestinationInitializationNode : SyncNode<IDataDestinationInitializationConfiguration>
    {
        public DataDestinationInitializationNode(ICommand<IDataDestinationInitializationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Initializing data destination";
            ParallelGroupName = "Multi node";
        }
    }
}
