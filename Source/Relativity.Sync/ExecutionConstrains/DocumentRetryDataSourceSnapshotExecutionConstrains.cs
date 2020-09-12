using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class DocumentRetryDataSourceSnapshotExecutionConstrains : IExecutionConstrains<IDocumentRetryDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDocumentRetryDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated && configuration.JobHistoryToRetryId != null);
		}
	}
}