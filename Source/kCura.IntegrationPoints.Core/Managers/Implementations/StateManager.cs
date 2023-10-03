using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
    public class StateManager : IStateManager
    {
        public ButtonStateDTO GetButtonState(
            ExportType exportType,
            ProviderType providerType,
            bool hasErrorViewPermissions,
            bool hasProfileAddPermission,
            bool calculationInProgress,
            string lastJobHistoryStatus,
            bool isIApiV2CustomProviderWorkflow)
        {
            bool runButtonEnabled = IsRunButtonEnable(lastJobHistoryStatus);
            bool viewErrorsLinkEnabled = IsViewErrorsLinkEnabled(providerType, hasErrorViewPermissions, lastJobHistoryStatus);
            bool retryErrorsButtonEnabled = IsRetryErrorsButtonEnabled(providerType, lastJobHistoryStatus);
            bool stopButtonEnabled = IsStopButtonEnabled(providerType, exportType, lastJobHistoryStatus, isIApiV2CustomProviderWorkflow);
            bool viewErrorsLinkVisible = IsViewErrorsLinkVisible(providerType, hasErrorViewPermissions);
            bool retryErrorsButtonVisible = IsRetryErrorsButtonVisible(providerType, exportType);
            bool saveAsProfileButtonVisible = IsSaveAsProfileButtonVisible(hasProfileAddPermission);
            bool downloadErrorFileLinkEnabled = IsDownloadErrorFileLinkEnabled(lastJobHistoryStatus);
            bool downloadErrorFileLinkVisible = IsDownloadErrorFileLinkVisible(providerType);
            bool calculateStatsButtonEnabled = IsCalculateStatisticsButtonEnabled(providerType, calculationInProgress);

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
                DownloadErrorFileLinkVisible = downloadErrorFileLinkVisible,
                CalculateStatisticsButtonEnabled = calculateStatsButtonEnabled
            };
        }

        private bool IsCalculateStatisticsButtonEnabled(ProviderType providerType, bool calculationInProgress)
        {
            return providerType == ProviderType.Relativity && !calculationInProgress;
        }

        private bool IsRunButtonEnable(string lastJobHistoryStatus)
        {
            return lastJobHistoryStatus.IsIn(
                StringComparison.InvariantCultureIgnoreCase,
                null,
                JobStatusChoices.JobHistoryCompleted.Name,
                JobStatusChoices.JobHistoryCompletedWithErrors.Name,
                JobStatusChoices.JobHistoryErrorJobFailed.Name,
                JobStatusChoices.JobHistoryStopped.Name,
                JobStatusChoices.JobHistoryValidationFailed.Name);
        }

        private bool IsViewErrorsLinkEnabled(ProviderType providerType, bool hasErrorViewPermissions, string lastJobHistoryStatus)
        {
            bool lastJobCondition = lastJobHistoryStatus.IsIn(
                StringComparison.InvariantCultureIgnoreCase,
                JobStatusChoices.JobHistoryErrorJobFailed.Name,
                JobStatusChoices.JobHistoryCompletedWithErrors.Name,
                JobStatusChoices.JobHistoryValidationFailed.Name);

            return providerType == ProviderType.Relativity && hasErrorViewPermissions && lastJobCondition;
        }

        private bool IsRetryErrorsButtonEnabled(ProviderType providerType, string lastJobHistoryStatus)
        {
            bool lastJobCondition = string.Equals(
                lastJobHistoryStatus,
                JobStatusChoices.JobHistoryCompletedWithErrors.Name,
                StringComparison.InvariantCultureIgnoreCase);

            return providerType == ProviderType.Relativity && lastJobCondition;
        }

        private bool IsStopButtonEnabled(ProviderType providerType, ExportType exportType, string lastJobHistoryStatus, bool isIApiV2CustomProviderWorkflow)
        {
            if (string.Equals(
                    lastJobHistoryStatus,
                    JobStatusChoices.JobHistoryPending.Name,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            bool isValidatingOrProcessing = lastJobHistoryStatus.IsIn(
                StringComparison.InvariantCultureIgnoreCase,
                JobStatusChoices.JobHistoryValidating.Name,
                JobStatusChoices.JobHistoryProcessing.Name);

            return isValidatingOrProcessing
                   && exportType != ExportType.ProductionSet
                   && (providerType.IsIn(ProviderType.Relativity, ProviderType.LoadFile) ||
                       isIApiV2CustomProviderWorkflow);
        }

        private bool IsViewErrorsLinkVisible(ProviderType providerType, bool hasErrorViewPermissions)
        {
            return providerType == ProviderType.Relativity && hasErrorViewPermissions;
        }

        private bool IsRetryErrorsButtonVisible(ProviderType providerType, ExportType exportType)
        {
            return providerType == ProviderType.Relativity && exportType != ExportType.View;
        }

        private bool IsSaveAsProfileButtonVisible(bool hasProfileAddPermission)
        {
            return hasProfileAddPermission;
        }

        private bool IsDownloadErrorFileLinkEnabled( string lastJobHistoryStatus)
        {
            return lastJobHistoryStatus.IsIn(
                StringComparison.InvariantCultureIgnoreCase,
                JobStatusChoices.JobHistoryErrorJobFailed.Name,
                JobStatusChoices.JobHistoryCompletedWithErrors.Name,
                JobStatusChoices.JobHistoryValidationFailed.Name);
        }

        private bool IsDownloadErrorFileLinkVisible(ProviderType providerType)
        {
            return providerType == ProviderType.ImportLoadFile;
        }
    }
}
