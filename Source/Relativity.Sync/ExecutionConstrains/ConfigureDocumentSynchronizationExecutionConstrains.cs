using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal class ConfigureDocumentSynchronizationExecutionConstrains : IExecutionConstrains<IConfigureDocumentSynchronizationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(false);
        }
    }
}
