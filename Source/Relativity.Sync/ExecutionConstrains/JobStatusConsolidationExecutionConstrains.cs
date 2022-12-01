using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.ExecutionConstrains
{
    internal class JobStatusConsolidationExecutionConstrains : IExecutionConstrains<IJobStatusConsolidationConfiguration>
    {
        private readonly IIAPIv2RunChecker _importV2RunChecker;

        public JobStatusConsolidationExecutionConstrains(IIAPIv2RunChecker importV2RunChecker)
        {
            _importV2RunChecker = importV2RunChecker;
        }

        public Task<bool> CanExecuteAsync(IJobStatusConsolidationConfiguration configuration, CancellationToken token)
        {
            return Task.FromResult(!_importV2RunChecker.ShouldBeUsed());
        }
    }
}
