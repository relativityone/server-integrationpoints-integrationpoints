using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class SourceWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Source workspace name contains an invalid character.";

		private readonly ISourceServiceFactoryForUser _serviceFactory;
		private readonly IWorkspaceNameValidator _workspaceNameValidator;
		private readonly ISyncLog _logger;

		public SourceWorkspaceNameValidator(ISourceServiceFactoryForUser serviceFactory, IWorkspaceNameValidator workspaceNameValidator, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_workspaceNameValidator = workspaceNameValidator;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogVerbose("Validating if source workspace does not contain invalid characters. {sourceWorkspaceArtifactId}", configuration.SourceWorkspaceArtifactId);
			ValidationResult result = new ValidationResult();

			bool isValidName = await _workspaceNameValidator.ValidateWorkspaceNameAsync(_serviceFactory, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
			if (!isValidName)
			{
				_logger.LogError("Destination workspace name is invalid.");
				result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
			}

			return result;
		}
	}
}