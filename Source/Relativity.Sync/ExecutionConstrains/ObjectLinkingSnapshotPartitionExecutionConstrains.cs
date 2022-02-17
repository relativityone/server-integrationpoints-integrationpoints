using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
