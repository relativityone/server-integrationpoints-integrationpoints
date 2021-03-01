using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	internal interface ICommand<in T> where T : IConfiguration
	{
		Task<bool> CanExecuteAsync(CancellationToken token);

		Task<ExecutionResult> ExecuteAsync(CompositeCancellationToken token);
	}
}