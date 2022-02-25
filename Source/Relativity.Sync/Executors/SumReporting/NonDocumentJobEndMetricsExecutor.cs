using Relativity.Sync.Configuration;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors.SumReporting
{
	internal class NonDocumentJobEndMetricsExecutor : IExecutor<INonDocumentJobEndMetricsConfiguration>
	{
		public Task<ExecutionResult> ExecuteAsync(INonDocumentJobEndMetricsConfiguration configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
