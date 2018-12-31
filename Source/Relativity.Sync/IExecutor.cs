using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface IExecutor<in T> where T : IConfiguration
	{
		Task ExecuteAsync(T configuration, CancellationToken token);
	}
}