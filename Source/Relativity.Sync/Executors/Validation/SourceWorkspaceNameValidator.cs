using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class SourceWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Source workspace name contains an invalid character.";

		private readonly IWorkspaceNameValidator _workspaceNameValidator;
		private readonly ISyncLog _logger;

		public SourceWorkspaceNameValidator(IWorkspaceNameValidator workspaceNameValidator, ISyncLog logger)
		{
			_workspaceNameValidator = workspaceNameValidator;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating if source workspace does not contain invalid characters. {sourceWorkspaceArtifactId}", configuration.SourceWorkspaceArtifactId);
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
			}

			return result;
		}
	}
}