using Relativity.Sync.Configuration;
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
