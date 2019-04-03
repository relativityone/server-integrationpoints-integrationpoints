using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DestinationWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Destination workspace name contains an invalid character.";

		private readonly IDestinationServiceFactoryForUser _serviceFactory;
		private readonly IWorkspaceNameValidator _workspaceNameValidator;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceNameValidator(IDestinationServiceFactoryForUser serviceFactory, IWorkspaceNameValidator workspaceNameValidator, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_workspaceNameValidator = workspaceNameValidator;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating if destination workspace does not contain invalid characters. {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(_serviceFactory, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				_logger.LogError("Source workspace name is invalid.");
				result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
			}

			return result;
		}
	}
}