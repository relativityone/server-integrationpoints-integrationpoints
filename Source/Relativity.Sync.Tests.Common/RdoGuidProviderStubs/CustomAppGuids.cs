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
            ReadItemsFieldGuid = new Guid("eeba8bb7-201c-47d1-a8ec-ec00e73755e2"),
            DestinationWorkspaceInformationGuid = new Guid("412677B9-7F13-44CF-9F9E-37A91ECEE420"),
            FailedItemsFieldGuid = new Guid("ABC708E9-4DB9-4B62-B2E9-3EEF0166A695"),
            TotalItemsFieldGuid = new Guid("B54407A6-26F5-48CE-9079-0A99A49C9CF3"),
            JobIdGuid = new Guid("801299fe-030c-4414-87f8-90626d2bc461"),
            StartTimeGuid = new Guid("34ca8b8c-e11c-435a-8797-1441bd5403f0"),
            EndTimeGuid = new Guid("5b6e9f49-633d-423d-9460-2a1b07d2881c"),
            StatusGuid = new Guid("7d69bcda-5c4c-4fbd-9a01-05ece982bb25")
        };

        public static IJobHistoryStatusProvider JobHistoryStatus => new JobHistoryStatusGuidProviderStub
        {
            CompletedGuid = new Guid("ee1e1ae9-2490-44b9-8424-b4200f9799c3"),
            CompletedWithErrorsGuid = new Guid("25e90af8-0ea4-4aa3-a246-c09c8138e146"),
            JobFailedGuid = new Guid("55039feb-de01-495b-9257-156dedfe7232"),
            ProcessingGuid = new Guid("352e211b-c85c-4aac-8d3b-eb38fb880b3d"),
            StoppedGuid = new Guid("baebac34-e587-4dec-a4c9-6da4db9918c6"),
            StoppingGuid = new Guid("ec47c22a-ef8d-4b28-b539-c4e98269ff6d"),
            SuspendedGuid = new Guid("6d19d402-072b-47b9-b845-afbeb47b417b"),
            SuspendingGuid = new Guid("cc8a6535-7a1c-447c-b110-8fde1d35ec5d"),
            ValidationFailedGuid = new Guid("dc3aa0cd-b212-4842-8898-7df34c438578"),
            ValidatingGuid = new Guid("d0bc4f28-db9c-48ce-aa11-8d0155bb441e")
        };

        public static IJobHistoryErrorGuidsProvider JobHistoryError => new JobHistoryErrorGuidsProviderStub
        {
            TypeGuid = new Guid("0C88EB14-14D5-4FED-85C1-56F5885D76B9"),
            JobHistoryRelationGuid = new Guid("61314C61-3504-4054-BF97-09F8A0D08B77"),
            SourceUniqueIdGuid = new Guid("ED83005C-0C04-43D5-B81D-E00ADFE0DF22"),
            ErrorMessagesGuid = new Guid("66C5E26F-DB42-4FD9-B2CF-14A2641C42E3"),
            ErrorStatusGuid = new Guid("CE53A2A4-FD29-437F-98A2-D0A4DB341929"),
            ErrorTypeGuid = new Guid("67936C04-A0A8-4C15-B055-F37D5978F4EF"),
            NameGuid = new Guid("ec885aaf-691a-4009-849c-d0e0449cf176"),
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
                JobHistory.ReadItemsFieldGuid,
                JobHistory.FailedItemsFieldGuid,
                JobHistory.TotalItemsFieldGuid,
                JobHistory.DestinationWorkspaceInformationGuid,
                JobHistory.StartTimeGuid,
                JobHistory.EndTimeGuid),
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
                    JobHistoryStatus.SuspendedGuid),
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
                JobHistoryError.NewStatusGuid),
            new DestinationWorkspaceOptions(
                DestinationWorkspace.TypeGuid,
                DestinationWorkspace.NameGuid,
                DestinationWorkspace.DestinationWorkspaceNameGuid,
                DestinationWorkspace.DestinationWorkspaceArtifactIdGuid,
                DestinationWorkspace.DestinationInstanceNameGuid,
                DestinationWorkspace.DestinationInstanceArtifactIdGuid,
                DestinationWorkspace.JobHistoryOnDocumentGuid,
                DestinationWorkspace.DestinationWorkspaceOnDocument));
    }
}
