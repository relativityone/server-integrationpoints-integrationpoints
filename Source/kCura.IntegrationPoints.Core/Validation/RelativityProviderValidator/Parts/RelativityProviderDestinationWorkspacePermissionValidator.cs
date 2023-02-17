using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderDestinationWorkspacePermissionValidator : IRelativityProviderDestinationWorkspacePermissionValidator
    {
        private readonly ArtifactPermission[] _expectedArtifactPermissions = { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create };

        private readonly IPermissionManager _permissionManager;

        public RelativityProviderDestinationWorkspacePermissionValidator(IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }

        public ValidationResult Validate(int destinationWorkspaceId, int destinationTypeId, bool createSavedSearch)
        {
            var result = new ValidationResult();

            if (!_permissionManager.UserHasPermissionToAccessWorkspace(destinationWorkspaceId))
            {
                result.Add(ValidationMessages.DestinationWorkspaceNoAccess);
                return result; // it does not make sense to validate other destination workspace permissions
            }

            if (!_permissionManager.UserCanImport(destinationWorkspaceId))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_IMPORT);
            }

            if (!_permissionManager.UserHasArtifactTypePermissions(destinationWorkspaceId, destinationTypeId, _expectedArtifactPermissions))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_RDO_PERMISSIONS);
            }

            if (createSavedSearch)
            {
                if (!_permissionManager.UserHasArtifactTypePermission(destinationWorkspaceId, (int)ArtifactType.Search, ArtifactPermission.Create))
                {
                    result.Add(ValidationMessages.MissingDestinationSavedSearchAddPermission);
                }
            }

            return result;
        }
    }
}
