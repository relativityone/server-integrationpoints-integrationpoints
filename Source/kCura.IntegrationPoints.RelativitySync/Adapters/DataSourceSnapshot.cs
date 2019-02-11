using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal sealed class DataSourceSnapshot : IExecutor<IDataSourceSnapshotConfiguration>, IExecutionConstrains<IDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task ExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
}