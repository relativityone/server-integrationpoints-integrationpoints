using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Configuration class for RDO representing JobHistoryError
    /// </summary>
    public class JobHistoryErrorOptions
    {
        /// <summary>
        /// GUID of the RDO type
        /// </summary>
        public Guid TypeGuid { get; private set;}
        
        /// <summary>
        /// Name (text)
        /// </summary>
        public Guid NameGuid { get; private set;}
        
        /// <summary>
        /// Source unique id (whole number)
        /// </summary>
        public Guid SourceUniqueIdGuid { get; private set;}
        
        /// <summary>
        /// Error message (text)
        /// </summary>
        public Guid ErrorMessageGuid { get; private set;}
        
        /// <summary>
        /// Timestamp (datetime)
        /// </summary>
        public Guid TimeStampGuid { get; private set;}
        
        /// <summary>
        /// Error type (Job or Item level error)
        /// </summary>
        public Guid ErrorTypeGuid { get; private set;}
        
        /// <summary>
        /// Stacktrace (text)
        /// </summary>
        public Guid StackTraceGuid { get; private set;}
        
        /// <summary>
        /// Error status (New, InProgress, Expired, Retried)
        /// <see cref="JobHistoryErrorStatusOptions"/>
        /// </summary>
        public Guid ErrorStatusGuid { get; private set;}
        
        /// <summary>
        /// Item Level Error value
        /// </summary>
        public Guid ItemLevelErrorChoiceGuid { get; private set;}
        
        /// <summary>
        /// Job Level Error value
        /// </summary>
        public Guid JobLevelErrorChoiceGuid { get; private set;}

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
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