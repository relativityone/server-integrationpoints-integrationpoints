using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal sealed class PermissionCheckExecutor : IExecutor<IPermissionsCheckConfiguration>
	{
		private readonly IEnumerable<IPermissionCheck> _validators;

		public PermissionCheckExecutor(IEnumerable<IPermissionCheck> validators)
		{
			_validators = validators;
		}

		public async Task<ExecutionResult> ExecuteAsync(IPermissionsCheckConfiguration configuration, CompositeCancellationToken token)
		{
			var validationResult = new ValidationResult();

			foreach (IPermissionCheck validator in _validators)
			{
				validationResult.Add(await validator.ValidateAsync(configuration).ConfigureAwait(false));
			}

			ExecutionResult executionResult = validationResult.IsValid
				? ExecutionResult.Success()
				: ExecutionResult.Failure(
					new ValidationException("Permission checks failed. See messages for more details.",
						validationResult));

			return executionResult;
		}
	}
}