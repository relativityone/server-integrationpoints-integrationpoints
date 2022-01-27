using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.ExecutionConstrains.SumReporting
{
	internal class NonDocumentJobStartMetricsExecutorConstrains : IExecutionConstrains<INonDocumentJobStartMetricsConfiguration>
	{
		public Task<bool> CanExecuteAsync(INonDocumentJobStartMetricsConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}
