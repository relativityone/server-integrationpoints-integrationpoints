using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class FailingExecutorStub<T> : IExecutor<T> where T : IConfiguration
	{
		public Task<ExecutionResult> ExecuteAsync(T configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Failure(new InvalidOperationException()));
		}
	}
}