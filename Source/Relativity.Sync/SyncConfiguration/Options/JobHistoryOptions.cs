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
        /// Constructor. All parameters are mandatory
        /// </summary>
        public JobHistoryOptions(Guid jobHistoryTypeGuid, Guid completedItemsCountGuid, Guid failedItemsCountGuid,
            Guid totalItemsCountGuid, Guid destinationWorkspaceInformationGuid)
        {
            JobHistoryTypeGuid = jobHistoryTypeGuid;
            CompletedItemsCountGuid = completedItemsCountGuid;
            FailedItemsCountGuid = failedItemsCountGuid;
            TotalItemsCountGuid = totalItemsCountGuid;
            DestinationWorkspaceInformationGuid = destinationWorkspaceInformationGuid;
        }
    }
}