using System;

namespace Relativity.Sync.Transfer.FileMovementService.Models
{
    /// <summary>
    /// The copy files response for FileMovementService
    /// </summary>
    internal class CopyListOfFilesResponse
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// Relativity Instance GUID
        /// </summary>
        public string RelativtyInstance { get; set; }

        /// <summary>
        /// Run Id of Pipeline run
        /// </summary>
        public string RunId { get; set; }

        /// <summary>
        /// Name of Pipeline in Azure Data Factory
        /// </summary>
        public string PipelineName { get; set; }
    }
}
