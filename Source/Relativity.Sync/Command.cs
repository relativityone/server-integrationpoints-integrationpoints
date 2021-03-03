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

		public Task<bool> CanExecuteAsync(CancellationToken token)
		{
			_logger.LogInformation("Checking if can execute step '{StepName}'", typeof(T).Name);
			_logger.LogInformation("Configuration properties of step '{StepName}': {@Configuration}", typeof(T).Name, _configuration);
			return _executionConstrains.CanExecuteAsync(_configuration, token);
		}

		public Task<ExecutionResult> ExecuteAsync(CompositeCancellationToken token)
		{
			return _executor.ExecuteAsync(_configuration, token);
		}
	}
}