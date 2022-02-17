using Relativity.Sync.Configuration;
using System.Threading;
using System.Threading.Tasks;

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
