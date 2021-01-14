﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Executors.PreValidation
{
	internal sealed class PreValidationExecutor : IExecutor<IPreValidationConfiguration>
	{
		private readonly IEnumerable<IPreValidator> _validators;
		private readonly ISyncLog _logger;

		public PreValidationExecutor(IEnumerable<IPreValidator> validators, ISyncLog logger)
		{
			_validators = validators;
			_logger = logger;
		}

		public async Task<ExecutionResult> ExecuteAsync(IPreValidationConfiguration configuration, CancellationToken token)
		{
			try
			{
				ValidationResult validationResult = new ValidationResult();
				foreach (var validator in _validators)
				{
					validationResult.Add(await validator.ValidateAsync(configuration, token).ConfigureAwait(false));
				}

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
	}
}