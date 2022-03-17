using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class StateManager : IStateManager
	{
		public ButtonStateDTO GetButtonState(ExportType exportType, ProviderType providerType, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasErrorViewPermissions,
			bool hasStoppableJobs, bool hasProfileAddPermission)
		{
			bool runButtonEnabled = IsRunButtonEnable(hasJobsExecutingOrInQueue);
			bool viewErrorsLinkEnabled = IsViewErrorsLinkEnabled(providerType, hasJobsExecutingOrInQueue, hasErrors, hasErrorViewPermissions);
			bool retryErrorsButtonEnabled = IsRetryErrorsButtonEnabled(providerType, hasJobsExecutingOrInQueue, hasErrors);
			bool stopButtonEnabled = IsStopButtonEnabled(hasStoppableJobs);
			bool viewErrorsLinkVisible = IsViewErrorsLinkVisible(providerType, hasErrorViewPermissions);
			bool retryErrorsButtonVisible = IsRetryErrorsButtonVisible(providerType, exportType);
			bool saveAsProfileButtonVisible = IsSaveAsProfileButtonVisible(hasProfileAddPermission);
			bool downloadErrorFileLinkEnabled = IsDownloadErrorFileLinkEnabled(hasErrors);
			bool downloadErrorFileLinkVisible = IsDownloadErrorFileLinkVisible(providerType);

			return new ButtonStateDTO
			{
				RetryErrorsButtonEnabled = retryErrorsButtonEnabled,
				ViewErrorsLinkVisible = viewErrorsLinkVisible,
				RetryErrorsButtonVisible = retryErrorsButtonVisible,
				ViewErrorsLinkEnabled = viewErrorsLinkEnabled,
				RunButtonEnabled = runButtonEnabled,
				StopButtonEnabled = stopButtonEnabled,
				SaveAsProfileButtonVisible = saveAsProfileButtonVisible,
				DownloadErrorFileLinkEnabled = downloadErrorFileLinkEnabled,
				DownloadErrorFileLinkVisible = downloadErrorFileLinkVisible
			};
		}

		private bool IsRunButtonEnable(bool hasJobsExecutingOrInQueue)
		{
			return !hasJobsExecutingOrInQueue;
		}

		private bool IsViewErrorsLinkEnabled(ProviderType providerType, bool hasJobsExecutingOrInQueue, bool hasErrors, bool hasErrorViewPermissions)
		{
			return (providerType == ProviderType.Relativity) && !hasJobsExecutingOrInQueue && hasErrors && hasErrorViewPermissions;
		}

		private bool IsRetryErrorsButtonEnabled(ProviderType providerType, bool hasJobsExecutingOrInQueue, bool hasErrors)
		{
			return (providerType == ProviderType.Relativity) && !hasJobsExecutingOrInQueue && hasErrors;
		}

		private bool IsStopButtonEnabled(bool hasStoppableJobs)
		{
			return hasStoppableJobs;
		}

		private bool IsViewErrorsLinkVisible(ProviderType providerType, bool hasErrorViewPermissions)
		{
			return (providerType == ProviderType.Relativity) && hasErrorViewPermissions;
		}

		private bool IsRetryErrorsButtonVisible(ProviderType providerType, ExportType exportType)
		{
			return providerType == ProviderType.Relativity && exportType != ExportType.View;
		}

		private bool IsSaveAsProfileButtonVisible(bool hasProfileAddPermission)
		{
			return hasProfileAddPermission;
		}

		private bool IsDownloadErrorFileLinkEnabled(bool hasErrors)
		{
			return hasErrors;
		}
		
		private bool IsDownloadErrorFileLinkVisible(ProviderType providerType)
		{
			return providerType == ProviderType.ImportLoadFile;
		}
	}
}