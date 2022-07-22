using Relativity.API;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Nodes
{
    internal sealed class ValidationNode : SyncNode<IValidationConfiguration>
    {
        public ValidationNode(ICommand<IValidationConfiguration> command, IAPILog logger) : base(command, logger)
        {
            Id = "Validating";
        }
    }
}
