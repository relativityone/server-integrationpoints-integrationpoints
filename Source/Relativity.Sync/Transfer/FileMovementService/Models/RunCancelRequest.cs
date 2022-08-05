using System;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// The run cancel request for FileMovementService
    /// </summary>
    public class RunCancelRequest
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// The Run Id
        /// </summary>
        public string RunId { get; set; }
    }
}
