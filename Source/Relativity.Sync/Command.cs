using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	internal sealed class Command<T> : ICommand<T> where T : IConfiguration
	{
		private readonly T _configuration;
		private readonly IExecutionConstrains<T> _executionConstrains;
		private readonly IExecutor<T> _executor;

		public Command(T configuration, IExecutionConstrains<T> executionConstrains, IExecutor<T> executor)
		{
			_configuration = configuration;
			_executionConstrains = executionConstrains;
			_executor = executor;
		}

		public async Task<bool> CanExecuteAsync(CancellationToken token)
		{
			return await _executionConstrains.CanExecuteAsync(_configuration, token).ConfigureAwait(false);
		}

		public async Task<ExecutionResult> ExecuteAsync(CancellationToken token)
		{
			return await _executor.ExecuteAsync(_configuration, token).ConfigureAwait(false);
		}
	}
}