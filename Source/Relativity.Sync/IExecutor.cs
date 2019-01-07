using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	internal interface IExecutor<in T> where T : IConfiguration
	{
		Task ExecuteAsync(T configuration, CancellationToken token);
	}
}