using System;
using kCura.IntegrationPoints.Data;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage.RdoGuidsProviders;
using Relativity.Sync.SyncConfiguration.Options;
using Constants = Relativity.Production.Constants;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal static class RdoConfiguration
    {
        public static RdoOptions GetRdoOptions() => new RdoOptions(
            new JobHistoryOptions(
                ObjectTypeGuids.JobHistoryGuid,
                JobHistoryFieldGuids.ItemsTransferredGuid,
                JobHistoryFieldGuids.ItemsWithErrorsGuid,
                JobHistoryFieldGuids.TotalItemsGuid,
                JobHistoryFieldGuids.DestinationWorkspaceInformationGuid
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