using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    public class JobHistoryOptions
    {
        public Guid JobHistoryTypeGuid { get; private set; }
        public Guid CompletedItemsCountGuid { get; private set;}
        public Guid FailedItemsCountGuid { get; private set;}
        public Guid TotalItemsCountGuid { get; private set;}
        public Guid DestinationWorkspaceInformationGuid { get; private set;}

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