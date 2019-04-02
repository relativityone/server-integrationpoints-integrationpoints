using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal sealed class SourceWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Source workspace name contains an invalid character.";

		private readonly IWorkspaceNameValidator _workspaceNameValidator;

		public SourceWorkspaceNameValidator(IWorkspaceNameValidator workspaceNameValidator)
		{
			_workspaceNameValidator = workspaceNameValidator;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				result.Add($"{_WORKSPACE_INVALID_NAME_MESSAGE}");
			}

			return result;
		}
	}
}