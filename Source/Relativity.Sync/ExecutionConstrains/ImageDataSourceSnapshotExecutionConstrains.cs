using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class ImageDataSourceSnapshotExecutionConstrains : IExecutionConstrains<IImageDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IImageDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated && configuration.IsImageJob);
		}
	}
}