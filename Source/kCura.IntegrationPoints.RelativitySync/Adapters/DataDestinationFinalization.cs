using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	/// <summary>
	///     Current implementation is empty.
	///     It will be used to mark productions as produced in destination workspace when pushing productions will be
	///     supported.
	/// </summary>
	internal sealed class DataDestinationFinalization : IExecutor<IDataDestinationFinalizationConfiguration>, IExecutionConstrains<IDataDestinationFinalizationConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task ExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
}