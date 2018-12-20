using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IExecutionConstrains<in T> where T : IConfiguration
	{
		Task<bool> CanExecuteAsync(T configuration, CancellationToken token);
	}
}