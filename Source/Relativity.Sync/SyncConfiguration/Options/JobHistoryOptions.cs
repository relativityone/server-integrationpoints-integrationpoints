using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Configuration class for RDO representing JobHistory
    /// </summary>
    public class JobHistoryOptions
    {
        /// <summary>
        /// GUID for RDO type
        /// </summary>
        public Guid JobHistoryTypeGuid { get; private set; }
        
        /// <summary>
        /// Completed items count (whole number)
        /// </summary>
        public Guid CompletedItemsCountGuid { get; private set; }

        /// <summary>
        /// Failed items count (whole number)
        /// </summary>
        public Guid FailedItemsCountGuid { get; private set; }

        /// <summary>
        /// Total items count (whole number)
        /// </summary>
        public Guid TotalItemsCountGuid { get; private set; }

        /// <summary>
        /// Destination workspace information (text)
        /// </summary>
        public Guid DestinationWorkspaceInformationGuid { get; private set; }

        /// <summary>
        /// Job ID
        /// </summary>
        public Guid JobIdGuid { get; }

        /// <summary>
        /// Start Time (Date Time)
        /// </summary>
        public Guid StartTimeGuid { get; private set; }

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public JobHistoryOptions(Guid jobHistoryTypeGuid, Guid completedItemsCountGuid, Guid failedItemsCountGuid,
            Guid totalItemsCountGuid, Guid destinationWorkspaceInformationGuid, Guid startTimeGuid, Guid jobIdGuid)
        {
            JobHistoryTypeGuid = jobHistoryTypeGuid;
            CompletedItemsCountGuid = completedItemsCountGuid;
            FailedItemsCountGuid = failedItemsCountGuid;
            TotalItemsCountGuid = totalItemsCountGuid;
            DestinationWorkspaceInformationGuid = destinationWorkspaceInformationGuid;
            StartTimeGuid = startTimeGuid;
            JobIdGuid = jobIdGuid;
        }
    }
}