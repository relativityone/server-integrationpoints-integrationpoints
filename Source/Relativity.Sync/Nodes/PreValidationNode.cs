using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class PreValidationNode : SyncNode<IPreValidationConfiguration>
    {
        public PreValidationNode(ICommand<IPreValidationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "PreValidating";
        }
    }
}
