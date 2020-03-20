using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.System.Core.Runner
{
	/// <summary>
	/// Empty implementation
	///
	/// It will be used to mark productions as produced in destination workspace when pushing productions will be supported.
	///
	/// Copied and aggregated form IntegrationPoints
	/// </summary>
	internal sealed class EmptyDataDestinationInitializationAndFinalization :
		IExecutor<IDataDestinationInitializationConfiguration>, IExecutionConstrains<IDataDestinationInitializationConfiguration>,
		IExecutor<IDataDestinationFinalizationConfiguration>, IExecutionConstrains<IDataDestinationFinalizationConfiguration>
	{
		public Task<bool> CanExecuteAsync(IDataDestinationInitializationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(IDataDestinationInitializationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}

		public Task<bool> CanExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(IDataDestinationFinalizationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
