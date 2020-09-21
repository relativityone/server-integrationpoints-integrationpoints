﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class ValidationExecutor : IExecutor<IValidationConfiguration>
	{
		private readonly IEnumerable<IValidator> _validators;
		private readonly IPipelineSelector _pipelineSelector;
		private readonly ISyncLog _logger;

		public ValidationExecutor(IEnumerable<IValidator> validators, IPipelineSelector pipelineSelector, ISyncLog logger)
		{
			_validators = validators;
			_pipelineSelector = pipelineSelector;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			try
			{
				ValidationResult validationResult = await ValidateAllAsync(configuration, token).ConfigureAwait(false);
				LogValidationErrors(validationResult);

				if (!validationResult.IsValid)
				{
					return ExecutionResult.Failure(new ValidationException("Validation failed. See ValidationResult property for more details.", validationResult));
				}

				return ExecutionResult.Success();
			}
			catch (Exception ex)
			{
				const string message = "Exception occurred during validation. See inner exception for more details.";
				_logger.LogError(ex, message);
				throw new ValidationException(message, ex);
			}
		}

		private async Task<ValidationResult> ValidateAllAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			ISyncPipeline syncPipeline = _pipelineSelector.GetPipeline();
			foreach (IValidator validator in _validators.Where(x => x.ShouldValidate(syncPipeline)))
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