using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class DestinationWorkspaceTagsCreationExecutionConstrains : IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(configuration.EnableTagging);
        }
    }
}
