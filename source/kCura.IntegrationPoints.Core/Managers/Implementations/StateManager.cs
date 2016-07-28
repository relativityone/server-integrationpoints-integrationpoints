using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasViewPermissions, bool hasStoppableJobs)
		{
			return new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue && hasErrors,
				ViewErrorsLinkEnabled = !hasJobsExecutingOrInQueue && hasErrors && hasViewPermissions,
				StopButtonEnabled = hasStoppableJobs
			};
		}
	}
}
