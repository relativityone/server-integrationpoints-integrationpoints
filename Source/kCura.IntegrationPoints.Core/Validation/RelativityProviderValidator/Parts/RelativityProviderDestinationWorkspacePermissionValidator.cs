using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

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
				var message = new ValidationMessage(
					Constants.IntegrationPoints.PermissionErrorCodes.DESTINATION_WORKSPACE_NO_ACCESS,
					Constants.IntegrationPoints.PermissionErrors.DESTINATION_WORKSPACE_NO_ACCESS
				);
				result.Add(message);
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
					result.Add(Constants.IntegrationPoints.PermissionErrorCodes.MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION, 
						Constants.IntegrationPoints.PermissionErrors.MISSING_DESTINATION_SAVED_SEARCH_ADD_PERMISSION);
				}
			}

			return result;
		}
	}
}
