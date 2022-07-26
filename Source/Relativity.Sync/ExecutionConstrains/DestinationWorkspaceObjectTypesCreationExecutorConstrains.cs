using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class DestinationWorkspaceObjectTypesCreationExecutorConstrains : IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDestinationWorkspaceObjectTypesCreationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}