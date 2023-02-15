using System;

namespace kCura.IntegrationPoints.Core.RelativitySync
{
    /// <summary>
    /// Thrown when sending sync job to sync app fails.
    /// </summary>
    public class SyncJobSendingException : Exception
    {
        public SyncJobSendingException(Exception innerException) : base("Sync job sending failed.", innerException)
        {
        }
    }
}
