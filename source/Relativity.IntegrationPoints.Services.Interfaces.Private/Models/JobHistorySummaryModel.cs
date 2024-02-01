using System;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a summary of job history, including details such as data, total available, and total documents pushed.
    /// </summary>
    public class JobHistorySummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobHistorySummaryModel"/> class.
        /// </summary>
        public JobHistorySummaryModel()
        {
            Data = new JobHistoryModel[0];
            TotalAvailable = 0;
            TotalDocumentsPushed = 0;
        }

        /// <summary>
        /// Gets or sets the data associated with the job history.
        /// </summary>
        public JobHistoryModel[] Data { get; set; }

        /// <summary>
        /// Gets or sets the total available records in the job history.
        /// </summary>
        public Int64 TotalAvailable { get; set; }

        /// <summary>
        /// Gets or sets the total number of documents pushed in the job history.
        /// </summary>
        public Int64 TotalDocumentsPushed { get; set; }
    }
}
