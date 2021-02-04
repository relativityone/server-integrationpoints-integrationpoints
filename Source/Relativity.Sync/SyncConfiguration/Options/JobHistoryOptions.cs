using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    public class JobHistoryOptions
    {
        public Guid JobHistoryTypeGuid { get; }
        public Guid CompletedItemsCountGuid { get; }
        public Guid FailedItemsCountGuid { get; }
        public Guid TotalItemsCountGuid { get; }

        public JobHistoryOptions(Guid jobHistoryTypeGuid, Guid completedItemsCountGuid, Guid failedItemsCountGuid,
            Guid totalItemsCountGuid)
        {
            JobHistoryTypeGuid = jobHistoryTypeGuid;
            CompletedItemsCountGuid = completedItemsCountGuid;
            FailedItemsCountGuid = failedItemsCountGuid;
            TotalItemsCountGuid = totalItemsCountGuid;
        }
    }
}