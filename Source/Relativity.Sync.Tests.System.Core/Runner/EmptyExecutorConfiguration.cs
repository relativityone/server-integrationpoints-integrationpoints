using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.System.Core.Runner
{
	/// <summary>
	/// Empty template implementation
	/// </summary>
	internal sealed class EmptyExecutorConfiguration<T> : IExecutor<T>, IExecutionConstrains<T>
		where T : IConfiguration
	{
		public Task<bool> CanExecuteAsync(T configuration, CancellationToken token)
		{
			return Task.FromResult(false);
		}

		public Task<ExecutionResult> ExecuteAsync(T configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}
