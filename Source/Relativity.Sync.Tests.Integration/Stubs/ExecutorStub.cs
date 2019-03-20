using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class ExecutorStub<T> : IExecutor<T> where T : IConfiguration
	{
		public Task ExecuteAsync(T configuration, CancellationToken token)
		{
			return Task.CompletedTask;
		}
	}
}