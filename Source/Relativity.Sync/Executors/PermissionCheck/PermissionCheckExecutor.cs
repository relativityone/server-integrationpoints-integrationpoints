using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.PermissionCheck
{
	internal sealed class PermissionCheckExecutor : IExecutor<IPermissionsCheckConfiguration>
	{
		private readonly IEnumerable<IPermissionCheck> _validators;
		private readonly IPipelineSelector _pipelineSelector;

		public PermissionCheckExecutor(IEnumerable<IPermissionCheck> validators, IPipelineSelector pipelineSelector)
		{
			_validators = validators;
			_pipelineSelector = pipelineSelector;
		}

		public async Task<ExecutionResult> ExecuteAsync(IPermissionsCheckConfiguration configuration, CompositeCancellationToken token)
		{
			var validationResult = new ValidationResult();

			foreach (IPermissionCheck validator in _validators
				.Where(x => x.ShouldValidate(_pipelineSelector.GetPipeline())))
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