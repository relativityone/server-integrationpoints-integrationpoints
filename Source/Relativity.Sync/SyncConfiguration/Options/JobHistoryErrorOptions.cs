using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    public class JobHistoryErrorOptions
    {
        public Guid TypeGuid { get; private set;}
        public Guid NameGuid { get; private set;}
        public Guid SourceUniqueIdGuid { get; private set;}
        public Guid ErrorMessageGuid { get; private set;}
        public Guid TimeStampGuid { get; private set;}
        public Guid ErrorTypeGuid { get; private set;}
        public Guid StackTraceGuid { get; private set;}
        public Guid ErrorStatusGuid { get; private set;}
        public Guid ItemLevelErrorChoiceGuid { get; private set;}
        public Guid JobLevelErrorChoiceGuid { get; private set;}

        public JobHistoryErrorOptions(Guid typeGuid, Guid nameGuid, Guid sourceUniqueIdGuid, Guid errorMessageGuid,
            Guid timeStampGuid, Guid errorTypeGuid, Guid stackTraceGuid, Guid errorStatusGuid,
            Guid itemLevelErrorChoiceGuid, Guid jobLevelErrorChoiceGuid)
        {
            TypeGuid = typeGuid;
            NameGuid = nameGuid;
            SourceUniqueIdGuid = sourceUniqueIdGuid;
            ErrorMessageGuid = errorMessageGuid;
            TimeStampGuid = timeStampGuid;
            ErrorTypeGuid = errorTypeGuid;
            StackTraceGuid = stackTraceGuid;
            ErrorStatusGuid = errorStatusGuid;
            ItemLevelErrorChoiceGuid = itemLevelErrorChoiceGuid;
            JobLevelErrorChoiceGuid = jobLevelErrorChoiceGuid;
        }
    }
}