using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class FailingExecutionConstrainsStub<T> : IExecutionConstrains<T> where T : IConfiguration
	{
		public Task<bool> CanExecuteAsync(T configuration, CancellationToken token)
		{
			throw new InvalidOperationException();
		}
	}
}