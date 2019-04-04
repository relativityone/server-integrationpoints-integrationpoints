using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ValidationExecutor : IExecutor<IValidationConfiguration>
	{
		private readonly IEnumerable<IValidator> _validatorsMap;
		private readonly ISyncLog _logger;

		public ValidationExecutor(IEnumerable<IValidator> validatorsMap, ISyncLog logger)
		{
			_validatorsMap = validatorsMap;
			_logger = logger;
		}

		public async Task ExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult validationResult = await ValidateExecutionConstraintsAsync(configuration, token).ConfigureAwait(false);
			LogValidationErrors(validationResult);

			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.ToString());
			}
		}

		private async Task<ValidationResult> ValidateExecutionConstraintsAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			foreach (IValidator validator in _validatorsMap)
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