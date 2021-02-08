using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    public class JobHistoryErrorStatusOptions
    {
        public Guid NewGuid { get; private set;}
        public Guid ExpiredGuid { get; private set;}
        public Guid InProgressGuid { get; private set;}
        public Guid RetriedGuid { get; private set;}

        public JobHistoryErrorStatusOptions(Guid newGuid, Guid expiredGuid, Guid inProgressGuid, Guid retriedGuid)
        {
            NewGuid = newGuid;
            ExpiredGuid = expiredGuid;
            InProgressGuid = inProgressGuid;
            RetriedGuid = retriedGuid;
        }
    }
}