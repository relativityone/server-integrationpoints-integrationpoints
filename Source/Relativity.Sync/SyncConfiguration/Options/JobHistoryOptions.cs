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
        public Guid JobHistoryTypeGuid { get; }

        /// <summary>
        /// Completed items count (whole number)
        /// </summary>
        public Guid CompletedItemsCountGuid { get; }

        /// <summary>
        /// Failed items count (whole number)
        /// </summary>
        public Guid FailedItemsCountGuid { get; }

        /// <summary>
        /// Total items count (whole number)
        /// </summary>
        public Guid TotalItemsCountGuid { get; }

        /// <summary>
        /// Destination workspace information (text)
        /// </summary>
        public Guid DestinationWorkspaceInformationGuid { get; }

        /// <summary>
        /// Job ID
        /// </summary>
        public Guid JobIdGuid { get; }

        /// <summary>
        /// Job status
        /// </summary>
        public Guid StatusGuid { get; }

        /// <summary>
        /// Start Time (Date Time)
        /// </summary>
        public Guid StartTimeGuid { get; }

        /// <summary>
        /// End Time (Date Time)
        /// </summary>
        public Guid EndTimeGuid { get; }

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public JobHistoryOptions(Guid jobHistoryTypeGuid, Guid jobIdGuid, Guid statusGuid, Guid completedItemsCountGuid, Guid failedItemsCountGuid,
            Guid totalItemsCountGuid, Guid destinationWorkspaceInformationGuid, Guid startTimeGuid, Guid endTimeGuid)
        {
            JobHistoryTypeGuid = jobHistoryTypeGuid;
            JobIdGuid = jobIdGuid;
            StatusGuid = statusGuid;
            CompletedItemsCountGuid = completedItemsCountGuid;
            FailedItemsCountGuid = failedItemsCountGuid;
            TotalItemsCountGuid = totalItemsCountGuid;
            DestinationWorkspaceInformationGuid = destinationWorkspaceInformationGuid;
            StartTimeGuid = startTimeGuid;
            EndTimeGuid = endTimeGuid;
        }
    }
}
