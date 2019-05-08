using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class SynchronizationExecutorConstrains : IExecutionConstrains<ISynchronizationConfiguration>
	{
		public async Task<bool> CanExecuteAsync(ISynchronizationConfiguration configuration, CancellationToken token)
		{
			await Task.Yield();
			return true;
		}
	}
}