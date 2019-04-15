using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class CompletedWithErrorsExecutorStub<T> : IExecutor<T> where T : IConfiguration
	{
		public Task<ExecutionResult> ExecuteAsync(T configuration, CancellationToken token)
		{
			return Task.FromResult(ExecutionResult.SuccessWithErrors(new InvalidOperationException()));
		}
	}
}
