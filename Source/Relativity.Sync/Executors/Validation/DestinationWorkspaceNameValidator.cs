using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
	internal sealed class DestinationWorkspaceNameValidator : IValidator
	{
		private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Destination workspace name contains an invalid character.";

		private readonly IDestinationServiceFactoryForUser _serviceFactory;
		private readonly IWorkspaceNameQuery _workspaceNameQuery;
		private readonly IWorkspaceNameValidator _workspaceNameValidator;
		private readonly ISyncLog _logger;

		public DestinationWorkspaceNameValidator(IDestinationServiceFactoryForUser serviceFactory, IWorkspaceNameQuery workspaceNameQuery, IWorkspaceNameValidator workspaceNameValidator, ISyncLog logger)
		{
			_serviceFactory = serviceFactory;
			_workspaceNameQuery = workspaceNameQuery;
			_workspaceNameValidator = workspaceNameValidator;
			_logger = logger;
		}

		public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
		{
			_logger.LogInformation("Validating if destination workspace does not contain invalid characters. Workspace Artifact ID: {destinationWorkspaceArtifactId}", configuration.DestinationWorkspaceArtifactId);
			ValidationResult result = new ValidationResult();
			try
			{
				string workspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(_serviceFactory, configuration.DestinationWorkspaceArtifactId, token).ConfigureAwait(false);
				bool isValidName = _workspaceNameValidator.Validate(workspaceName, configuration.DestinationWorkspaceArtifactId, token);
				if (!isValidName)
				{
					_logger.LogError("Destination workspace name is invalid.");
					result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
				}
			}
			catch (Exception ex)
			{
				string message = "Error occurred while querying for workspace artifact ID: {0}";
				_logger.LogError(ex, message, configuration.DestinationWorkspaceArtifactId);
				throw;
			}

			return result;
		}

		public bool ShouldValidate(ISyncPipeline pipeline) => true;
	}
}