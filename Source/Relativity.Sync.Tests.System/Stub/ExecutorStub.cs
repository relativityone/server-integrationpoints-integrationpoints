using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal sealed class ExecutorStub<T> : IExecutor<T> where T : IConfiguration
	{
		private readonly List<Type> _types;

		public ExecutorStub(List<Type> types)
		{
			_types = types;
		}

		public Task ExecuteAsync(T configuration, CancellationToken token)
		{
			_types.Add(typeof(T));
			return Task.CompletedTask;
		}
	}
}