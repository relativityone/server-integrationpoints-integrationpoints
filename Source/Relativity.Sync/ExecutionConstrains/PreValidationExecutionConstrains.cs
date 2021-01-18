using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class PreValidationExecutionConstrains : IExecutionConstrains<IPreValidationConfiguration>
	{
		public Task<bool> CanExecuteAsync(IPreValidationConfiguration configuration, CancellationToken token)
		{
			return Task.FromResult(true);
		}
	}
}
