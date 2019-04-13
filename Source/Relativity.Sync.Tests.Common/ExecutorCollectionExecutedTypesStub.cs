using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common
{
	internal sealed class ExecutorCollectionExecutedTypesStub<T> : IExecutor<T> where T : IConfiguration
	{
		private readonly List<Type> _types;

		public ExecutorCollectionExecutedTypesStub(List<Type> types)
		{
			_types = types;
		}

		public Task<ExecutionResult> ExecuteAsync(T configuration, CancellationToken token)
		{
			_types.Add(typeof(T));
			return Task.FromResult(ExecutionResult.Success());
		}
	}
}