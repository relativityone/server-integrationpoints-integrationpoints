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
		private readonly ISyncLog _logger;

		public Command(T configuration, IExecutionConstrains<T> executionConstrains, IExecutor<T> executor, ISyncLog logger)
		{
			_configuration = configuration;
			_executionConstrains = executionConstrains;
			_executor = executor;
			_logger = logger;
		}

		public async Task<bool> CanExecuteAsync(CancellationToken token)
		{
			_logger.LogInformation("Starting execution of step with configuration {configurationName}.", typeof(T).Name, _configuration);
			return await _executionConstrains.CanExecuteAsync(_configuration, token).ConfigureAwait(false);
		}

		public async Task<ExecutionResult> ExecuteAsync(CancellationToken token)
		{
			return await _executor.ExecuteAsync(_configuration, token).ConfigureAwait(false);
		}
	}
}