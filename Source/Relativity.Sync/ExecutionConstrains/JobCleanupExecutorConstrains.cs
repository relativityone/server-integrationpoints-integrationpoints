using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class JobCleanupExecutorConstrains : IExecutionConstrains<IJobCleanupConfiguration>
    {
        public Task<bool> CanExecuteAsync(IJobCleanupConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}