using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains.SumReporting
{
	internal class ImageJobStartMetricsExecutorConstrains : IExecutionConstrains<IImageJobStartMetricsConfiguration>
	{
		public Task<bool> CanExecuteAsync(IImageJobStartMetricsConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}
