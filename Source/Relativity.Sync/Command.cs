using Relativity.API;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync
{
	internal sealed class Command<T> : ICommand<T> where T : IConfiguration
	{
		private readonly string _stepName;
		private readonly T _configuration;
		private readonly IExecutionConstrains<T> _executionConstrains;
		private readonly IExecutor<T> _executor;
		private readonly IAPILog _logger;
		
		public Command(T configuration, IExecutionConstrains<T> executionConstrains, IExecutor<T> executor, IAPILog logger)
		{
			_stepName = typeof(T).Name;
			_configuration = configuration;
			_executionConstrains = executionConstrains;
			_executor = executor;
			_logger = logger;
		}

		public async Task<bool> CanExecuteAsync(CancellationToken token)
		{
			_logger.LogInformation("Configuration properties of step '{StepName}': {@Configuration}", _stepName, _configuration);

			_logger.LogInformation("Checking if can execute step '{StepName}'", _stepName);
			bool canExecute = await _executionConstrains.CanExecuteAsync(_configuration, token).ConfigureAwait(false);
			_logger.LogInformation("Can execute step '{StepName}': {CanExecute}", _stepName, canExecute);
			return canExecute;
		}

		public async Task<ExecutionResult> ExecuteAsync(CompositeCancellationToken token)
		{
			_logger.LogInformation("Executing step '{StepName}'", _stepName);
			ExecutionResult executionResult = await _executor.ExecuteAsync(_configuration, token).ConfigureAwait(false);
			_logger.LogInformation("Step '{StepName}' execution result: {@ExecutionResult}", _stepName, executionResult);
			return executionResult;
		}
	}
}
