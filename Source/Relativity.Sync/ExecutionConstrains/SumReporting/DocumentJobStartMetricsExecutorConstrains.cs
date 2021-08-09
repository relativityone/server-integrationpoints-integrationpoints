using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains.SumReporting
{
	internal class DocumentJobStartMetricsExecutorConstrains : IExecutionConstrains<IDocumentJobStartMetricsConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDocumentJobStartMetricsConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}
