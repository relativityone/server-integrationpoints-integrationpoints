using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class DestinationWorkspaceSavedSearchCreationExecutionConstrains : IExecutionConstrains<IDestinationWorkspaceSavedSearchCreationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, CancellationToken token)
        {
            bool shouldExecute = configuration.CreateSavedSearchForTags && !configuration.IsSavedSearchArtifactIdSet;
            return Task.FromResult(shouldExecute);
        }
    }
}
