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
		public ValidationResult Validate(int workspaceId)
		{
			var result = new ValidationResult();

			if (!_workspaceManager.WorkspaceExists(workspaceId))
			{
				var message = new ValidationMessage(
					Constants.IntegrationPoints.ValidationErrorCodes.DESTINATION_WORKSPACE_NOT_AVAILABLE,
					Constants.IntegrationPoints.ValidationErrors.DESTINATION_WORKSPACE_NOT_AVAILABLE
				); // TODO move to separe class

				result.Add(message);
			}
			return result;
		}
	}
}
