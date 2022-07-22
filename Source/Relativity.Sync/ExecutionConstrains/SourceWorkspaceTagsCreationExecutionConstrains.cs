using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class SourceWorkspaceTagsCreationExecutionConstrains : IExecutionConstrains<ISourceWorkspaceTagsCreationConfiguration>
    {
        public Task<bool> CanExecuteAsync(ISourceWorkspaceTagsCreationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}