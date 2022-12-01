using System;

namespace Relativity.Sync.Configuration
{
    internal interface IRdoGuidConfiguration : IConfiguration
    {
        IJobHistoryRdoGuidsProvider JobHistory { get; }

        IJobHistoryStatusProvider JobHistoryStatus { get; }

        IJobHistoryErrorGuidsProvider JobHistoryError { get; }

        IDestinationWorkspaceTagGuidProvider DestinationWorkspace { get; }
    }

    internal interface IJobHistoryRdoGuidsProvider
    {
        Guid TypeGuid { get; }

        Guid CompletedItemsFieldGuid { get; }

        Guid FailedItemsFieldGuid { get; }

        Guid TotalItemsFieldGuid { get; }

        Guid DestinationWorkspaceInformationGuid { get; }

        Guid JobIdGuid { get; }

        Guid StartTimeGuid { get; }

        Guid EndTimeGuid { get; }

        Guid StatusGuid { get; }
    }

    internal interface IJobHistoryStatusProvider
    {
        Guid CompletedGuid { get; }

        Guid CompletedWithErrorsGuid { get; }

        Guid JobFailedGuid { get; }

        Guid ProcessingGuid { get; }

        Guid StoppedGuid { get; }

        Guid StoppingGuid { get; }

        Guid SuspendedGuid { get; }

        Guid SuspendingGuid { get; }

        Guid ValidatingGuid { get; }

        Guid ValidationFailedGuid { get; }
    }

    internal interface IJobHistoryErrorGuidsProvider
    {
        Guid TypeGuid { get; }

        Guid ErrorMessagesGuid { get; }

        Guid ErrorStatusGuid { get; }

        Guid ErrorTypeGuid { get; }

        Guid NameGuid { get; }

        Guid SourceUniqueIdGuid { get; }

        Guid StackTraceGuid { get; }

        Guid TimeStampGuid { get; }

        Guid ItemLevelErrorGuid { get; }

        Guid JobLevelErrorGuid { get; }

        Guid JobHistoryRelationGuid { get; }

        Guid NewStatusGuid { get; }
    }

    internal interface IDestinationWorkspaceTagGuidProvider
    {
        Guid TypeGuid { get; }

        Guid NameGuid { get; }

        Guid DestinationWorkspaceNameGuid { get; }

        Guid DestinationWorkspaceArtifactIdGuid { get; }

        Guid DestinationInstanceNameGuid { get; }

        Guid DestinationInstanceArtifactIdGuid { get; }

        Guid JobHistoryOnDocumentGuid { get; }

        Guid DestinationWorkspaceOnDocument { get; }
    }
}
