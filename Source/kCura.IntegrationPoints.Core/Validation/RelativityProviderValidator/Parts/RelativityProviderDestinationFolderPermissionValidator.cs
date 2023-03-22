using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

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
            ValidationResult result = new ValidationResult();

            bool canAddDocument = _permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Document, destinationFolderArtifactId, ArtifactPermission.Create);

            if (!canAddDocument)
            {
                result.Add(PermissionErrors.DESTINATION_DOCUMENT_NO_CREATE_PERMISSION);
            }

            if (useFolderPathInfo && !_permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Folder, destinationFolderArtifactId, ArtifactPermission.Create))
            {
                result.Add(PermissionErrors.DESTINATION_FOLDER_NO_CREATE_PERMISSION);
            }

            if (moveExistingDocuments && !_permissionManager.UserHasArtifactInstancePermission(_workspaceId, (int)ArtifactType.Document, destinationFolderArtifactId, ArtifactPermission.Delete))
            {
                result.Add(PermissionErrors.DESTINATION_DOCUMENT_NO_DELETE_PERMISSION);
            }

            return result;
        }
    }
}
