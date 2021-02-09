using System;

namespace Relativity.Sync.Configuration
{
    internal interface IRdoGuidConfiguration : IConfiguration
    {
        IJobHistoryRdoGuidsProvider JobHistory { get; }
        IJobHistoryErrorGuidsProvider JobHistoryError { get; }
    }

    internal interface IJobHistoryRdoGuidsProvider
    {
        Guid TypeGuid { get; }
        Guid CompletedItemsFieldGuid { get; }
        Guid FailedItemsFieldGuid { get; }
        Guid TotalItemsFieldGuid { get; }
        Guid DestinationWorkspaceInformationGuid { get; }
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
}