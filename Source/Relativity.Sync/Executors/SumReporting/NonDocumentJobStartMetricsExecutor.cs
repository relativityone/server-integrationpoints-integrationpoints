using Relativity.Sync.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class NonDocumentJobStartMetricsExecutor : IExecutor<INonDocumentJobStartMetricsConfiguration>
	{
		public Task<ExecutionResult> ExecuteAsync(INonDocumentJobStartMetricsConfiguration configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
