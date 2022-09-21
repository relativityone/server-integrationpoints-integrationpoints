using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal class BatchDataSourcePreparationExecutionConstrains : IExecutionConstrains<IBatchDataSourcePreparationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IBatchDataSourcePreparationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(false);
        }
    }
}
