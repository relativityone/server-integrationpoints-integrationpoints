using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class ImageRetryDataSourceSnapshotExecutionConstrains : IExecutionConstrains<IImageRetryDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IImageRetryDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated && configuration.JobHistoryToRetryId != null && configuration.IsImageJob);
		}
	}
}