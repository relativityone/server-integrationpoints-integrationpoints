using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal class NonDocumentObjectDataSourceSnapshotExecutor : IExecutor<INonDocumentDataSourceSnapshotConfiguration>
	{
		public Task<ExecutionResult> ExecuteAsync(INonDocumentDataSourceSnapshotConfiguration configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
