using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	/// <summary> 
	///     Current implementation is empty. 
	///     It will be used to create productions in destination workspace when pushing productions will be supported. 
	/// </summary> 
	internal sealed class DataDestinationInitializationExecutor : IExecutor<IDataDestinationInitializationConfiguration>
	{
		public Task<ExecutionResult> ExecuteAsync(IDataDestinationInitializationConfiguration configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}