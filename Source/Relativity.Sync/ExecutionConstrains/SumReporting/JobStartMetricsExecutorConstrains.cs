using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains.SumReporting
{
	internal class JobStartMetricsExecutorConstrains : IExecutionConstrains<ISumReporterConfiguration>
	{
		public Task<bool> CanExecuteAsync(ISumReporterConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}