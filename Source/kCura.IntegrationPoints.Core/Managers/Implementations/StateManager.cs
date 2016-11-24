using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public ButtonStateDTO GetButtonState(Constants.SourceProvider sourceProvider, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasErrorViewPermissions,
			bool hasStoppableJobs, bool hasProfileAddPermission)
		{
			bool runButtonEnabled = IsRunButtonEnable(hasJobsExecutingOrInQueue);
			bool viewErrorsLinkEnabled = IsViewErrorsLinkEnabled(sourceProvider, hasJobsExecutingOrInQueue, hasErrors, hasErrorViewPermissions);
			bool retryErrorsButtonEnabled = IsRetryErrorsButtonEnabled(sourceProvider, hasJobsExecutingOrInQueue, hasErrors);
			bool stopButtonEnabled = IsStopButtonEnabled(hasStoppableJobs);
			bool viewErrorsLinkVisible = IsViewErrorsLinkVisible(sourceProvider, hasErrorViewPermissions);
			bool retryErrorsButtonVisible = IsRetryErrorsButtonVisible(sourceProvider);
			bool saveAsProfileButtonVisible = IsSaveAsProfileButtonVisible(hasProfileAddPermission);

			return new ButtonStateDTO
			{
				RetryErrorsButtonEnabled = retryErrorsButtonEnabled,
				ViewErrorsLinkVisible = viewErrorsLinkVisible,
				RetryErrorsButtonVisible = retryErrorsButtonVisible,
				ViewErrorsLinkEnabled = viewErrorsLinkEnabled,
				RunButtonEnabled = runButtonEnabled,
				StopButtonEnabled = stopButtonEnabled,
				SaveAsProfileButtonVisible = saveAsProfileButtonVisible
			};
		}

		private bool IsRunButtonEnable(bool hasJobsExecutingOrInQueue)
		{
			return !hasJobsExecutingOrInQueue;
		}

		private bool IsViewErrorsLinkEnabled(Constants.SourceProvider sourceProvider, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasErrorViewPermissions)
		{
			return (sourceProvider == Constants.SourceProvider.Relativity) && !hasJobsExecutingOrInQueue && hasErrors && hasErrorViewPermissions;
		}

		private bool IsRetryErrorsButtonEnabled(Constants.SourceProvider sourceProvider, bool hasJobsExecutingOrInQueue, bool hasErrors)
		{
			return (sourceProvider == Constants.SourceProvider.Relativity) && !hasJobsExecutingOrInQueue && hasErrors;
		}

		private bool IsStopButtonEnabled(bool hasStoppableJobs)
		{
			return hasStoppableJobs;
		}

		private bool IsViewErrorsLinkVisible(Constants.SourceProvider sourceProvider, bool hasErrorViewPermissions)
		{
			return (sourceProvider == Constants.SourceProvider.Relativity) && hasErrorViewPermissions;
		}

		private bool IsRetryErrorsButtonVisible(Constants.SourceProvider sourceProvider)
		{
			return sourceProvider == Constants.SourceProvider.Relativity;
		}

		private bool IsSaveAsProfileButtonVisible(bool hasProfileAddPermission)
		{
			return hasProfileAddPermission;
		}
	}
}