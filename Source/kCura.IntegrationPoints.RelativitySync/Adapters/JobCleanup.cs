using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class JobCleanup : IExecutor<IJobCleanupConfiguration>, IExecutionConstrains<IJobCleanupConfiguration>
	{
		public Task<bool> CanExecuteAsync(IJobCleanupConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(IJobCleanupConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}