using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderDestinationFolderPermissionValidator : IRelativityProviderDestinationFolderPermissionValidator
    {
        private readonly IPermissionManager _permissionManager;
        private readonly int _workspaceId;

        public RelativityProviderDestinationFolderPermissionValidator(int workspaceId, IPermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
            _workspaceId = workspaceId;
        }

        public ValidationResult Validate(int destinationFolderArtifactId, bool useFolderPathInfo, bool moveExistingDocuments)
        {
            bool canAddDocument = _permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Document,
                destinationFolderArtifactId, ArtifactPermission.Create);
            bool isValid = canAddDocument;

            if (useFolderPathInfo && isValid)
            {
                bool canAddSubfolders = _permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Folder,
                    destinationFolderArtifactId, ArtifactPermission.Create);
                isValid &= canAddSubfolders;
            }

            if (moveExistingDocuments && isValid)
            {
                bool canDeleteDocument = _permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Document,
                    destinationFolderArtifactId, ArtifactPermission.Delete);
                isValid &= canDeleteDocument;
            }

            return isValid
                ? new ValidationResult()
                : new ValidationResult(ValidationMessages.MissingDestinationFolderItemLevelPermissions);
        }
    }
}
