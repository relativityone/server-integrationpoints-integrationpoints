using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public RelativityButtonStateDTO GetRelativityProviderButtonState(bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasViewPermissions, bool hasStoppableJobs)
		{
			return new RelativityButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				RetryErrorsButtonEnabled = !hasJobsExecutingOrInQueue && hasErrors,
				ViewErrorsLinkEnabled = !hasJobsExecutingOrInQueue && hasErrors && hasViewPermissions,
				StopButtonEnabled = hasStoppableJobs
			};
		}

		public ButtonStateDTO GetButtonState(bool hasJobsExecutingOrInQueue, bool hasStoppableJobs)
		{
			return new ButtonStateDTO()
			{
				RunNowButtonEnabled = !hasJobsExecutingOrInQueue,
				StopButtonEnabled = hasStoppableJobs
			};
		}
	}
}
