using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	/// <summary>
	///     Current implementation is empty.
	///     It will be used to create productions in destination workspace when pushing productions will be supported.
	/// </summary>
	internal sealed class DataDestinationInitialization : IExecutor<IDataDestinationInitializationConfiguration>, IExecutionConstrains<IDataDestinationInitializationConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDataDestinationInitializationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(IDataDestinationInitializationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}