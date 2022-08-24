using System;

namespace Relativity.Sync.Transfer.FileMovementService.Models
{
    /// <summary>
    /// The run status request for FileMovementService
    /// </summary>
    internal class FmsBatchStatusInfo
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// The Run Id
        /// </summary>
        public string RunId { get; set; }

        /// <summary>
        /// The batch status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A message that provides more detail for the status
        /// </summary>
        public string StatusMessage { get; set; }
    }
}
