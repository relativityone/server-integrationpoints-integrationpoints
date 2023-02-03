using kCura.IntegrationPoints.Data;
using Relativity.Sync.SyncConfiguration.Options;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal static class RdoConfiguration
    {
        public static RdoOptions GetRdoOptions() => new RdoOptions(
            new JobHistoryOptions(
                ObjectTypeGuids.JobHistoryGuid,
                JobHistoryFieldGuids.JobIDGuid,
                JobHistoryFieldGuids.JobStatusGuid,
                JobHistoryFieldGuids.ItemsTransferredGuid,
                JobHistoryFieldGuids.ItemsReadGuid,
                JobHistoryFieldGuids.ItemsWithErrorsGuid,
                JobHistoryFieldGuids.TotalItemsGuid,
                JobHistoryFieldGuids.DestinationWorkspaceInformationGuid,
                JobHistoryFieldGuids.StartTimeUTCGuid,
                JobHistoryFieldGuids.EndTimeUTCGuid
            ),
            new JobHistoryStatusOptions(
                JobStatusChoices.JobHistoryValidatingGuid,
                JobStatusChoices.JobHistoryValidationFailedGuid,
                JobStatusChoices.JobHistoryProcessingGuid,
                JobStatusChoices.JobHistoryCompletedGuid,
                JobStatusChoices.JobHistoryCompletedWithErrorsGuid,
                JobStatusChoices.JobHistoryErrorJobFailedGuid,
                JobStatusChoices.JobHistoryStoppingGuid,
                JobStatusChoices.JobHistoryStoppedGuid,
                JobStatusChoices.JobHistorySuspendingGuid,
                JobStatusChoices.JobHistorySuspendedGuid
            ),
            new JobHistoryErrorOptions(
                ObjectTypeGuids.JobHistoryErrorGuid,
                JobHistoryErrorFieldGuids.NameGuid,
                JobHistoryErrorFieldGuids.SourceUniqueIDGuid,
                JobHistoryErrorFieldGuids.ErrorGuid,
                JobHistoryErrorFieldGuids.TimestampUTCGuid,
                JobHistoryErrorFieldGuids.ErrorTypeGuid,
                JobHistoryErrorFieldGuids.StackTraceGuid,
                JobHistoryErrorFieldGuids.JobHistoryGuid,
                JobHistoryErrorFieldGuids.ErrorStatusGuid,
                ErrorTypeChoices.JobHistoryErrorItemGuid,
                ErrorTypeChoices.JobHistoryErrorJobGuid,
                ErrorStatusChoices.JobHistoryErrorNewGuid
            ),
            new DestinationWorkspaceOptions(
                ObjectTypeGuids.DestinationWorkspaceGuid,
                DestinationWorkspaceFieldGuids.NameGuid,
                DestinationWorkspaceFieldGuids.DestinationWorkspaceNameGuid,
                DestinationWorkspaceFieldGuids.DestinationWorkspaceArtifactIDGuid,
                DestinationWorkspaceFieldGuids.DestinationInstanceNameGuid,
                DestinationWorkspaceFieldGuids.DestinationInstanceArtifactIDGuid,
                DocumentFieldGuids.JobHistoryGuid,
                DocumentFieldGuids.RelativityDestinationCaseGuid
            )
        );
    }
}
