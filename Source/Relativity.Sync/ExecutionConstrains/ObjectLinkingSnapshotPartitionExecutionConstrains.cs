using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal class ObjectLinkingSnapshotPartitionExecutionConstrains : IExecutionConstrains<IObjectLinkingSnapshotPartitionConfiguration>
	{
		public Task<bool> CanExecuteAsync(IObjectLinkingSnapshotPartitionConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(configuration.LinkingExportExists);
		}
	}
}
