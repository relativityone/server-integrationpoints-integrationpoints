using System;

namespace Relativity.Sync.Transfer.FileMovementService.Models
{
    /// <summary>
    /// The run status request for FileMovementService
    /// </summary>
    internal class RunStatusRequest
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
