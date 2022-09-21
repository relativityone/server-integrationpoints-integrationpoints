using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal class ConfigureDocumentSynchronizationExecutor : IExecutor<IConfigureDocumentSynchronizationConfiguration>
    {
        public Task<ExecutionResult> ExecuteAsync(IConfigureDocumentSynchronizationConfiguration configuration, CompositeCancellationToken token)
        {
            return Task.FromResult(default(ExecutionResult));
        }
    }
}
