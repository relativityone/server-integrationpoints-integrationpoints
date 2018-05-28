using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts
{
	public class RelativityProviderSourceWorkspacePermissionValidator : IRelativityProviderSourceWorkspacePermissionValidator
	{
		private readonly IPermissionManager _permissionManager;

		public RelativityProviderSourceWorkspacePermissionValidator(IPermissionManager permissionManager)
		{
			_permissionManager = permissionManager;
		}
		public ValidationResult Validate(int sourceWorkspaceId)
		{
			var result = new ValidationResult();

			if (!_permissionManager.UserCanExport(sourceWorkspaceId))
			{
				result.Add(Constants.IntegrationPoints.PermissionErrors.SOURCE_WORKSPACE_NO_EXPORT);
			}

			if (!_permissionManager.UserCanEditDocuments(sourceWorkspaceId))
			{
				result.Add(Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS);
			}

			return result;
		}
	}
}

