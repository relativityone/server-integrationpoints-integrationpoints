using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public ButtonStateDTO GetButtonState(int workspaceId, int integrationPointId, bool hasJobsExecutingOrInQueue, bool hasErrors)
		{
			return new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue && hasErrors,
				ViewErrorsLinkEnabled = !hasJobsExecutingOrInQueue && hasErrors,
			};
		}
	}
}
