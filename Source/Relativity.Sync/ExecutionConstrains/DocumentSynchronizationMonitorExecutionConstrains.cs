using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal class DocumentSynchronizationMonitorExecutionConstrains : IExecutionConstrains<IDocumentSynchronizationMonitorConfiguration>
    {
        public Task<bool> CanExecuteAsync(IDocumentSynchronizationMonitorConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(false);
        }
    }
}
