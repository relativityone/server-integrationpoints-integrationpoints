using System;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Choice;
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
            ChoiceRef lastJobHistoryStatus,
            isIApiV2CustomProviderWorkflow)
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

        private bool IsRunButtonEnable(ChoiceRef lastJobHistoryStatus)
        {
            return lastJobHistoryStatus.EqualsToAnyChoice(
                null,
                JobStatusChoices.JobHistoryCompleted,
                JobStatusChoices.JobHistoryCompletedWithErrors,
                JobStatusChoices.JobHistoryErrorJobFailed,
                JobStatusChoices.JobHistoryStopped,
                JobStatusChoices.JobHistoryValidationFailed);
        }

        private bool IsViewErrorsLinkEnabled(ProviderType providerType, bool hasErrorViewPermissions, ChoiceRef lastJobHistoryStatus)
        {
            bool lastJobCondition = lastJobHistoryStatus.EqualsToAnyChoice(
                JobStatusChoices.JobHistoryErrorJobFailed,
                JobStatusChoices.JobHistoryCompletedWithErrors,
                JobStatusChoices.JobHistoryValidationFailed);

            return providerType == ProviderType.Relativity && hasErrorViewPermissions && lastJobCondition;
        }

        private bool IsRetryErrorsButtonEnabled(ProviderType providerType, ChoiceRef lastJobHistoryStatus)
        {
            return providerType == ProviderType.Relativity
                   && lastJobHistoryStatus.EqualsToAnyChoice(JobStatusChoices.JobHistoryCompletedWithErrors);
        }

        private bool IsStopButtonEnabled(ProviderType providerType, ExportType exportType, ChoiceRef lastJobHistoryStatus)
        {
            if (lastJobHistoryStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending))
            {
                return true;
            }

            bool isValidatingOrProcessing = lastJobHistoryStatus.EqualsToAnyChoice(
                JobStatusChoices.JobHistoryValidating,
                JobStatusChoices.JobHistoryProcessing);

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

        private bool IsDownloadErrorFileLinkEnabled(ChoiceRef lastJobHistoryStatus)
        {
            return lastJobHistoryStatus.EqualsToAnyChoice(
                JobStatusChoices.JobHistoryErrorJobFailed,
                JobStatusChoices.JobHistoryCompletedWithErrors,
                JobStatusChoices.JobHistoryValidationFailed);
        }

        private bool IsDownloadErrorFileLinkVisible(ProviderType providerType)
        {
            return providerType == ProviderType.ImportLoadFile;
        }
    }
}
