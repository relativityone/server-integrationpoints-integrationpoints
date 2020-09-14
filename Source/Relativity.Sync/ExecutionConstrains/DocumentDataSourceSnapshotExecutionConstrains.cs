using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class DocumentDataSourceSnapshotExecutionConstrains : IExecutionConstrains<IDocumentDataSourceSnapshotConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDocumentDataSourceSnapshotConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(!configuration.IsSnapshotCreated);
		}
	}
}