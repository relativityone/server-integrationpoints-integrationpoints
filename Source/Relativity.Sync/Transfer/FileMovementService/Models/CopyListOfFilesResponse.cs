using System;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// The copy files response for FileMovementService
    /// </summary>
    public class CopyListOfFilesResponse
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// Relativty Instance GUID
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
