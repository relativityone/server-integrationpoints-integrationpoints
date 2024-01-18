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
        public Guid JobHistoryTypeGuid { get; set; }

        /// <summary>
        /// Completed items count (whole number)
        /// </summary>
        public Guid CompletedItemsCountGuid { get; set; }

        /// <summary>
        /// Read items count (whole number)
        /// </summary>
        public Guid ReadItemsCountGuid { get; set; }

        /// <summary>
        /// Failed items count (whole number)
        /// </summary>
        public Guid FailedItemsCountGuid { get; set; }

        /// <summary>
        /// Total items count (whole number)
        /// </summary>
        public Guid TotalItemsCountGuid { get; set; }

        /// <summary>
        /// Destination workspace information (text)
        /// </summary>
        public Guid DestinationWorkspaceInformationGuid { get; set; }

        /// <summary>
        /// Job ID
        /// </summary>
        public Guid JobIdGuid { get; set; }

        /// <summary>
        /// Job status
        /// </summary>
        public Guid StatusGuid { get; set; }

        /// <summary>
        /// Start Time (Date Time)
        /// </summary>
        public Guid StartTimeGuid { get; set; }

        /// <summary>
        /// End Time (Date Time)
        /// </summary>
        public Guid EndTimeGuid { get; set; }

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public JobHistoryOptions(
            Guid jobHistoryTypeGuid,
            Guid jobIdGuid,
            Guid statusGuid,
            Guid completedItemsCountGuid,
            Guid readItemsCountGuid,
            Guid failedItemsCountGuid,
            Guid totalItemsCountGuid,
            Guid destinationWorkspaceInformationGuid,
            Guid startTimeGuid,
            Guid endTimeGuid)
        {
            JobHistoryTypeGuid = jobHistoryTypeGuid;
            JobIdGuid = jobIdGuid;
            StatusGuid = statusGuid;
            CompletedItemsCountGuid = completedItemsCountGuid;
            ReadItemsCountGuid = readItemsCountGuid;
            FailedItemsCountGuid = failedItemsCountGuid;
            TotalItemsCountGuid = totalItemsCountGuid;
            DestinationWorkspaceInformationGuid = destinationWorkspaceInformationGuid;
            StartTimeGuid = startTimeGuid;
            EndTimeGuid = endTimeGuid;
        }
    }
}
