using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class FailingExecutorStub<T> : IExecutor<T> where T : IConfiguration
	{
		public Task<ExecutionResult> ExecuteAsync(T configuration, CompositeCancellationToken token)
		{
			return Task.FromResult(ExecutionResult.Failure(new InvalidOperationException()));
		}
	}
}