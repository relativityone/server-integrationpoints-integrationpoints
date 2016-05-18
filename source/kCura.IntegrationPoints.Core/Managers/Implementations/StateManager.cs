using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool hasJobsExecutingOrInQueue, bool permissionSuccess, bool hasErrors)
		{
			return new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue && permissionSuccess,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue && permissionSuccess && hasErrors,
				ViewErrorsLinkEnabled = !hasJobsExecutingOrInQueue && hasErrors,
			};
		}
	}
}
