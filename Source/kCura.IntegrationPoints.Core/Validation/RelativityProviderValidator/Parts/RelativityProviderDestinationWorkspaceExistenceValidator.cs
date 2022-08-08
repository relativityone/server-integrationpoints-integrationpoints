using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderDestinationWorkspaceExistenceValidator : IRelativityProviderDestinationWorkspaceExistenceValidator
    {
        private readonly IWorkspaceManager _workspaceManager;

        public RelativityProviderDestinationWorkspaceExistenceValidator(IWorkspaceManager workspaceManager)
        {
            _workspaceManager = workspaceManager;
        }
        public ValidationResult Validate(SourceConfiguration sourceConfiguration)
        {
            var result = new ValidationResult();

            if (!_workspaceManager.WorkspaceExists(sourceConfiguration.TargetWorkspaceArtifactId))
            {
                bool isFederatedInstance = sourceConfiguration.FederatedInstanceArtifactId != null;
                ValidationMessage message = isFederatedInstance
                    ? ValidationMessages.FederatedInstanceDestinationWorkspaceNotAvailable
                    : ValidationMessages.DestinationWorkspaceNotAvailable;
                result.Add(message);
            }
            return result;
        }
    }
}
