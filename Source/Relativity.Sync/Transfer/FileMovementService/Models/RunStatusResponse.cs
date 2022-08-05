using System;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// The run status response model for FileMovementService
    /// </summary>
    public class RunStatusResponse
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// A status code representing the result of the request.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A message that provides more detail for the status
        /// </summary>
        public string Message { get; set; }
    }
}
