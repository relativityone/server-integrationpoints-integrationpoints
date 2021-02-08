using System;

namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Configuration class for choice representing JobHistoryError.ErrorStatus <see cref="JobHistoryErrorOptions.ErrorStatusGuid"/>
    /// </summary>
    public class JobHistoryErrorStatusOptions
    {
        /// <summary>
        /// New status
        /// </summary>
        public Guid NewGuid { get; private set;}
        
        /// <summary>
        /// Expired status
        /// </summary>
        public Guid ExpiredGuid { get; private set;}
        
        /// <summary>
        /// InProgress status 
        /// </summary>
        public Guid InProgressGuid { get; private set;}
        
        /// <summary>
        /// Retried status 
        /// </summary>
        public Guid RetriedGuid { get; private set;}

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public JobHistoryErrorStatusOptions(Guid newGuid, Guid expiredGuid, Guid inProgressGuid, Guid retriedGuid)
        {
            NewGuid = newGuid;
            ExpiredGuid = expiredGuid;
            InProgressGuid = inProgressGuid;
            RetriedGuid = retriedGuid;
        }
    }
}