using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderSourceWorkspacePermissionValidator : IRelativityProviderSourceWorkspacePermissionValidator
    {
        private readonly IPermissionManager _permissionManager;

        public RelativityProviderSourceWorkspacePermissionValidator(IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }

        public ValidationResult Validate(int sourceWorkspaceId, int artifactTypeId)
        {
            var result = new ValidationResult();

            if (!_permissionManager.UserCanExport(sourceWorkspaceId))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
            }

            if (!_permissionManager.UserHasArtifactTypePermissions(sourceWorkspaceId, artifactTypeId, new [] { ArtifactPermission.View }))
            {
                result.Add(Constants.IntegrationPoints.PermissionErrors.MISSING_SOURCE_RDO_PERMISSIONS);
            }

            if (artifactTypeId == (int)ArtifactType.Document && !_permissionManager.UserCanEditDocuments(sourceWorkspaceId))
            {
                result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
            }

            return result;
        }
    }
}

