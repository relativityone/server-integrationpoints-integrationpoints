using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Destination workspace name contains an invalid character.";

		private readonly IWorkspaceNameValidator _workspaceNameValidator;

		public DestinationWorkspaceNameValidator(IWorkspaceNameValidator workspaceNameValidator)
		{
			_workspaceNameValidator = workspaceNameValidator;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				result.Add($"{_WORKSPACE_INVALID_NAME_MESSAGE}");
			}

			return result;
		}
	}
}