using System;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents the history of a job, including details such as items transferred, end time, destination workspace, destination instance, files size, and overwrite information.
    /// </summary>
    public class JobHistoryModel
    {
        /// <summary>
        /// Gets or sets the number of items transferred during the job.
        /// </summary>
        public int ItemsTransferred { get; set; }

        /// <summary>
        /// Gets or sets the end time of the job in Coordinated Universal Time (UTC).
        /// </summary>
        public DateTime EndTimeUTC { get; set; }

        /// <summary>
        /// Gets or sets the destination workspace for the job.
        /// </summary>
        public string DestinationWorkspace { get; set; }

        /// <summary>
        /// Gets or sets the destination instance for the job.
        /// </summary>
        public string DestinationInstance { get; set; }

        /// <summary>
        /// Gets or sets the total size of files transferred during the job.
        /// </summary>
        public string FilesSize { get; set; }

        /// <summary>
        /// Gets or sets the overwrite information for the job.
        /// </summary>
        public string Overwrite { get; set; }
    }
}
