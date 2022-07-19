using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
    internal class JobStatusConsolidationExecutionConstrains : IExecutionConstrains<IJobStatusConsolidationConfiguration>
    {
        public Task<bool> CanExecuteAsync(IJobStatusConsolidationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(true);
        }
    }
}