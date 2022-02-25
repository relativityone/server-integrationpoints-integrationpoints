using Relativity.Sync.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.ExecutionConstrains.SumReporting
{
	internal class NonDocumentJobEndMetricsExecutorConstrains : IExecutionConstrains<INonDocumentJobEndMetricsConfiguration>
	{
		public Task<bool> CanExecuteAsync(INonDocumentJobEndMetricsConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}
