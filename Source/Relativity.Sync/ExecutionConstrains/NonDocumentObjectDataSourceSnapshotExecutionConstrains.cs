using Relativity.Sync.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.ExecutionConstrains
{
	internal class NonDocumentObjectDataSourceSnapshotExecutionConstrains : IExecutionConstrains<INonDocumentDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(INonDocumentDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated);
		}
	}
}
