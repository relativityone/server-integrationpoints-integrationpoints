using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal class RetryDataSourceSnapshotExecutionConstrains : IExecutionConstrains<IRetryDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IRetryDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated && configuration.JobHistoryToRetryId != null);
		}
	}
}