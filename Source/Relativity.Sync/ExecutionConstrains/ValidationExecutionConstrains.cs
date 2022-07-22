using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class ValidationExecutionConstrains : IExecutionConstrains<IValidationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}