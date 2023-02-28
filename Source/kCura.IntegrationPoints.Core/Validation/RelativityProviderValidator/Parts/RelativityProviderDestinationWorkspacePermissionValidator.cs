using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Models;
using Relativity;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
    public class RelativityProviderDestinationWorkspacePermissionValidator : IRelativityProviderDestinationWorkspacePermissionValidator
    {
        private readonly ArtifactPermission[] _expectedArtifactPermissions = { ArtifactPermission.View, ArtifactPermission.Edit, ArtifactPermission.Create };

        private readonly IPermissionManager _permissionManager;
        private readonly IRepositoryFactory _repositoryFactory;

        public RelativityProviderDestinationWorkspacePermissionValidator(
            IPermissionManager permissionManager,
            IRepositoryFactory repositoryFactory)
        {
            _permissionManager = permissionManager;
            _repositoryFactory = repositoryFactory;
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
                IObjectTypeRepository objectTypeRepository = _repositoryFactory.GetObjectTypeRepository(destinationWorkspaceId);

                ObjectTypeDTO objectType = objectTypeRepository.GetObjectType(destinationTypeId);

                result.Add(Constants.IntegrationPoints.PermissionErrors.MissingDestinationRdoPermission(objectType.Name));
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
