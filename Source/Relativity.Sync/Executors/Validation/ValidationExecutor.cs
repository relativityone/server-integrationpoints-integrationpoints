using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ValidationExecutor : IExecutor<IValidationConfiguration>
	{
		private readonly IEnumerable<IValidator> _validators;
		private readonly ISyncLog _logger;

		public ValidationExecutor(IEnumerable<IValidator> validators, ISyncLog logger)
		{
			_validators = validators;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			try
			{
				ValidationResult validationResult = await ValidateAll(configuration, token).ConfigureAwait(false);
				LogValidationErrors(validationResult);

				if (!validationResult.IsValid)
				{
					return ExecutionResult.Failure(new ValidationException(validationResult));
				}

				return ExecutionResult.Success();
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during validation.";
				_logger.LogError(ex, message);
				return ExecutionResult.Failure(new ValidationException(message, ex));
			}
		}

		private async Task<ValidationResult> ValidateAll(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			foreach (IValidator validator in _validators)
			{
				result.Add(await validator.ValidateAsync(configuration, token).ConfigureAwait(false));
			}

			return result;
		}

		private void LogValidationErrors(ValidationResult validationResult)
		{
			if (validationResult.Messages == null)
			{
				return;
			}

			foreach (ValidationMessage validationMessage in validationResult.Messages)
			{
				_logger.LogError("Integration Point validation failed. ErrorCode: {errorCode}, ShortMessage: {shortMessage}", validationMessage.ErrorCode, validationMessage.ShortMessage);
			}
		}
	}
}