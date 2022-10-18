using System;

namespace Relativity.Sync.Transfer.FileMovementService.Models
{
    /// <summary>
    /// The copy files request for FileMovementService
    /// </summary>
    internal class CopyListOfFilesRequest
    {
        /// <summary>
        /// TraceId for tracking
        /// </summary>
        public Guid TraceId { get; set; }

        /// <summary>
        /// Source Directory on Azure Data Lake Storage Gen2 without container name
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Destination Directory on Azure Data Lake Storage Gen2 without container name
        /// </summary>
        public string DestinationPath { get; set; }

        /// <summary>
        /// Path without container name to a CSV file with a list files, this file should be located in Source Directory on Azure Data Lake Storage Gen2
        /// </summary>
        public string PathToListOfFiles { get; set; }

        /// <summary>
        /// Endpoint URL to send request for copy list of files logic.
        /// </summary>
        public string EndpointURL { get; set; }
    }
}
