using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal class DataSourceSnapshotExecutionConstrains : IExecutionConstrains<IDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated);
		}
	}
}
