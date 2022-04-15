using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

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
