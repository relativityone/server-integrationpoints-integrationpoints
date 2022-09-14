using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal class DocumentSynchronizationMonitorExecutor : IExecutor<IDocumentSynchronizationMonitorConfiguration>
    {
        public Task<ExecutionResult> ExecuteAsync(IDocumentSynchronizationMonitorConfiguration configuration, CompositeCancellationToken token)
        {
            return Task.FromResult(default(ExecutionResult));
        }
    }
}
