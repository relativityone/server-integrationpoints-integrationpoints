using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage.RdoGuidsProviders;
using Relativity.Sync.SyncConfiguration.Options;

namespace kCura.IntegrationPoints.RelativitySync
{
    internal static class RdoConfiguration
    {
        // Job history
        public static readonly Guid JobHistoryTypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");

        public static readonly Guid JobHistoryCompletedItemsFieldGuid =
            new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c");

        public static readonly Guid JobHistoryDestinationWorkspaceInformationGuid =
            new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b");

        public static readonly Guid JobHistoryFailedItemsFieldGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e");

        public static readonly Guid JobHistoryTotalItemsFieldGuid =
            new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b");

        // JobHistoryError
        public static readonly Guid JobHistoryErrorTypeGuid = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");

        public static readonly Guid JobHistoryErrorJobHistoryRelationGuid =
            new Guid("8b747b91-0627-4130-8e53-2931ffc4135f");

        public static readonly Guid JobHistoryErrorSourceUniqueIdGuid =
            new Guid("5519435e-ee82-4820-9546-f1af46121901");

        public static readonly Guid JobHistoryErrorErrorMessagesGuid = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
        public static readonly Guid JobHistoryErrorErrorStatusGuid = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678");
        public static readonly Guid JobHistoryErrorErrorTypeGuid = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973");
        public static readonly Guid JobHistoryErrorNameGuid = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66");
        public static readonly Guid JobHistoryErrorStackTraceGuid = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
        public static readonly Guid JobHistoryErrorTimeStampGuid = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F");

        public static readonly Guid JobHistoryErrorItemLevelErrorGuid =
            new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B");

        public static readonly Guid JobHistoryErrorJobLevelErrorGuid = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B");
        public static readonly Guid JobHistoryErrorNewStatusGuid = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A");

        public static RdoOptions GetRdoOptions() => new RdoOptions(
            new JobHistoryOptions(
                JobHistoryTypeGuid,
                JobHistoryCompletedItemsFieldGuid,
                JobHistoryFailedItemsFieldGuid,
                JobHistoryTotalItemsFieldGuid,
                JobHistoryDestinationWorkspaceInformationGuid
            ),
            new JobHistoryErrorOptions(
                JobHistoryErrorTypeGuid,
                JobHistoryErrorNameGuid,
                JobHistoryErrorSourceUniqueIdGuid,
                JobHistoryErrorErrorMessagesGuid,
                JobHistoryErrorTimeStampGuid,
                JobHistoryErrorErrorTypeGuid,
                JobHistoryErrorStackTraceGuid,
                JobHistoryErrorJobHistoryRelationGuid,
                JobHistoryErrorErrorStatusGuid,
                JobHistoryErrorItemLevelErrorGuid,
                JobHistoryErrorJobLevelErrorGuid,
                JobHistoryErrorNewStatusGuid
            )
        );
    }
}