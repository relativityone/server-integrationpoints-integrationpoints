using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class JobStatusConsolidation : IExecutor<IJobStatusConsolidationConfiguration>, IExecutionConstrains<IJobStatusConsolidationConfiguration>
	{
		public Task<bool> CanExecuteAsync(IJobStatusConsolidationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(IJobStatusConsolidationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}