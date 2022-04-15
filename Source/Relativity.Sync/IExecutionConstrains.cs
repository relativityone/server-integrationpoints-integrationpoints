using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	internal interface IExecutionConstrains<in T> where T : IConfiguration
	{
		Task<bool> CanExecuteAsync(T configuration, CancellationToken token);
	}
}