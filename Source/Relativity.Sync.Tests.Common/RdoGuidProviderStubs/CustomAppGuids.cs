using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal static class CustomAppGuids
    {
        public static IJobHistoryRdoGuidsProvider JobHistory => new JobHistoryRdoGuidsProviderStub
        {
            TypeGuid = new Guid("DF0A4E86-251E-4B21-870D-265C9B00B0F5"),
            CompletedItemsFieldGuid = new Guid("EC869E59-933F-44C8-9E9F-5F1C4619B1AA"),
            DestinationWorkspaceInformationGuid = new Guid("412677B9-7F13-44CF-9F9E-37A91ECEE420"),
            FailedItemsFieldGuid = new Guid("ABC708E9-4DB9-4B62-B2E9-3EEF0166A695"),
            TotalItemsFieldGuid = new Guid("B54407A6-26F5-48CE-9079-0A99A49C9CF3")
        };

        public static IJobHistoryStatusProvider JobHistoryStatus => new JobHistoryStatusGuidProviderStub
        {
            CompletedGuid = new Guid("c7d1eb34-166e-48d0-bce7-0be0df43511c"),
            CompletedWithErrorsGuid = new Guid("c0f4a2b2-499e-45bc-96d7-f8bc25e18b37"),
            JobFailedGuid = new Guid("3152ece9-40e6-44dd-afc8-1004f55dfb63"),
            ProcessingGuid = new Guid("bb170e53-2264-4708-9b00-86156187ed54"),
            StoppedGuid = new Guid("a29c5bcb-d3a6-4f81-877a-2a6556c996c3"),
            StoppingGuid = new Guid("97c1410d-864d-4811-857b-952464872baa"),
            SuspendedGuid = new Guid("f219e060-d7e1-4666-964d-f229a1a13baa"),
            SuspendingGuid = new Guid("c65658c3-79ea-4762-b78e-85d9f38785b6"),
            ValidationFailedGuid = new Guid("d0b43a57-bdc8-4c14-b2f0-2928ae4f750a"),
            ValidatingGuid = new Guid("6a2dcef5-5826-4f61-9bac-59fef879ebc2")
        };

        public static IJobHistoryErrorGuidsProvider JobHistoryError => new JobHistoryErrorGuidsProviderStub
        {
            TypeGuid = new Guid("0C88EB14-14D5-4FED-85C1-56F5885D76B9"),
            JobHistoryRelationGuid = new Guid("61314C61-3504-4054-BF97-09F8A0D08B77"),
            SourceUniqueIdGuid = new Guid("ED83005C-0C04-43D5-B81D-E00ADFE0DF22"),
            ErrorMessagesGuid = new Guid("66C5E26F-DB42-4FD9-B2CF-14A2641C42E3"),
            ErrorStatusGuid = new Guid("CE53A2A4-FD29-437F-98A2-D0A4DB341929"),
            ErrorTypeGuid = new Guid("67936C04-A0A8-4C15-B055-F37D5978F4EF"),
            NameGuid = new Guid("8027D593-1BC3-49E5-8AEB-10AEF0475ECC"),
            StackTraceGuid = new Guid("BE5DAF5C-5C95-47C6-988C-0F53704199F9"),
            TimeStampGuid = new Guid("EA356DA1-C992-4A8E-8BCE-BED966477106"),
            ItemLevelErrorGuid = new Guid("6D9DAB63-4A26-4E36-A2FC-2EDD73B8C29C"),
            JobLevelErrorGuid = new Guid("F83155DD-63BD-4972-AAE0-D3A84A129178"),
            NewStatusGuid = new Guid("A8E3F1C5-2070-40CD-8199-25C2FDBEF2D2")
        };

        public static IDestinationWorkspaceTagGuidProvider DestinationWorkspace =>
            new DestinationWorkspaceTagGuidProviderStub
            {
                NameGuid = new Guid("5E9084F9-8AAA-4942-BA41-65E782A008F6"),
                TypeGuid = new Guid("FCECD3C7-B666-4EC7-BB3B-36440EDC6074"),
                DestinationInstanceNameGuid = new Guid("4370468C-75C2-4181-B9A7-2E55F1D7C77A"),
                DestinationWorkspaceNameGuid = new Guid("022B8A93-32CB-4839-B476-09B38F8A779A"),
                DestinationInstanceArtifactIdGuid = new Guid("08BFA030-0D08-4B8D-8B2B-B8E60C416BC0"),
                DestinationWorkspaceArtifactIdGuid = new Guid("5DC8B850-9BC1-4C47-AD9B-D4BDB4E0327F"),
                DestinationWorkspaceOnDocument = new Guid("A569431D-4A1F-40C2-836C-DAEE95F87B38"),
                JobHistoryOnDocumentGuid = new Guid("36AED8BA-66A6-4ED5-B6EA-F3CD85504BC5")
            };

        public static RdoOptions Guids => new RdoOptions(
            new JobHistoryOptions(
                JobHistory.TypeGuid,
                JobHistory.JobIdGuid,
                JobHistory.StatusGuid,
                JobHistory.CompletedItemsFieldGuid,
                JobHistory.FailedItemsFieldGuid,
                JobHistory.TotalItemsFieldGuid,
                JobHistory.DestinationWorkspaceInformationGuid,
                JobHistory.StartTimeGuid,
                JobHistory.EndTimeGuid
            ),
            new JobHistoryStatusOptions(
                JobHistoryStatus.ValidatingGuid,
                JobHistoryStatus.ValidationFailedGuid,
                JobHistoryStatus.ProcessingGuid,
                JobHistoryStatus.CompletedGuid,
                JobHistoryStatus.CompletedWithErrorsGuid,
                JobHistoryStatus.JobFailedGuid,
                JobHistoryStatus.StoppingGuid,
                JobHistoryStatus.StoppedGuid,
                JobHistoryStatus.SuspendingGuid,
                JobHistoryStatus.SuspendedGuid
            ),
            new JobHistoryErrorOptions(
                JobHistoryError.TypeGuid,
                JobHistoryError.NameGuid,
                JobHistoryError.SourceUniqueIdGuid,
                JobHistoryError.ErrorMessagesGuid,
                JobHistoryError.TimeStampGuid,
                JobHistoryError.ErrorTypeGuid,
                JobHistoryError.StackTraceGuid,
                JobHistoryError.JobHistoryRelationGuid,
                JobHistoryError.ErrorStatusGuid,
                JobHistoryError.ItemLevelErrorGuid,
                JobHistoryError.JobLevelErrorGuid,
                JobHistoryError.NewStatusGuid
            ),
            new DestinationWorkspaceOptions(
                DestinationWorkspace.TypeGuid,
                DestinationWorkspace.NameGuid,
                DestinationWorkspace.DestinationWorkspaceNameGuid,
                DestinationWorkspace.DestinationWorkspaceArtifactIdGuid,
                DestinationWorkspace.DestinationInstanceNameGuid,
                DestinationWorkspace.DestinationInstanceArtifactIdGuid,
                DestinationWorkspace.JobHistoryOnDocumentGuid,
                DestinationWorkspace.DestinationWorkspaceOnDocument
            )
        );
    }
}
