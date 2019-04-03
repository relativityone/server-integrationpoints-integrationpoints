using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DestinationWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Destination workspace name contains an invalid character.";

		private readonly IWorkspaceNameValidator _workspaceNameValidator;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceNameValidator(IWorkspaceNameValidator workspaceNameValidator, ISyncLog logger)
		{
			_workspaceNameValidator = workspaceNameValidator;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating if destination workspace does not contain invalid characters. {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				_logger.LogError("Source workspace name is invalid.");
				result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
			}

			return result;
		}
	}
}