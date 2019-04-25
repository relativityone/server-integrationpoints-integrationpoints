using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class SnapshotPartition : IExecutor<ISnapshotPartitionConfiguration>, IExecutionConstrains<ISnapshotPartitionConfiguration>
	{
		public Task<bool> CanExecuteAsync(ISnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(ISnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}