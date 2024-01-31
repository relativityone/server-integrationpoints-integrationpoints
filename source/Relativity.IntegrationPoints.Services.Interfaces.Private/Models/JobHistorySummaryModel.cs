using System;

namespace Relativity.IntegrationPoints.Services
{
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

        public JobHistoryModel[] Data { get; set; }

        public Int64 TotalAvailable { get; set; }

        public Int64 TotalDocumentsPushed { get; set; }
    }
}
