using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Pipelines;

namespace Relativity.Sync.Executors.Validation
{
    internal sealed class SourceWorkspaceNameValidator : IValidator
    {
        private const string _WORKSPACE_INVALID_NAME_MESSAGE = "Source workspace name contains an invalid character.";

        private readonly ISourceServiceFactoryForUser _serviceFactoryForUser;
        private readonly IWorkspaceNameQuery _workspaceNameQuery;
        private readonly IWorkspaceNameValidator _workspaceNameValidator;
        private readonly IAPILog _logger;

        public SourceWorkspaceNameValidator(ISourceServiceFactoryForUser serviceFactoryForUser, IWorkspaceNameQuery workspaceNameQuery, IWorkspaceNameValidator workspaceNameValidator, IAPILog logger)
        {
            _serviceFactoryForUser = serviceFactoryForUser;
            _workspaceNameQuery = workspaceNameQuery;
            _workspaceNameValidator = workspaceNameValidator;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateAsync(IValidationConfiguration configuration, CancellationToken token)
        {
            _logger.LogInformation("Validating if source workspace does not contain invalid characters. Workspace Artifact ID: {sourceWorkspaceArtifactId}", configuration.SourceWorkspaceArtifactId);
            ValidationResult result = new ValidationResult();
            try
            {
                string workspaceName = await _workspaceNameQuery.GetWorkspaceNameAsync(_serviceFactoryForUser, configuration.SourceWorkspaceArtifactId, token).ConfigureAwait(false);
                bool isValidName = _workspaceNameValidator.Validate(workspaceName, configuration.SourceWorkspaceArtifactId, token);
                if (!isValidName)
                {
                    _logger.LogError("Source workspace name is invalid.");
                    result.Add(_WORKSPACE_INVALID_NAME_MESSAGE);
                }
            }
            catch (Exception ex)
            {
                string message = "Error occurred while validating source workspace name ID: {0}";
                _logger.LogError(ex, message, configuration.SourceWorkspaceArtifactId);
                throw;
            }

            return result;
        }

        public bool ShouldValidate(ISyncPipeline pipeline) => true;
    }
}
