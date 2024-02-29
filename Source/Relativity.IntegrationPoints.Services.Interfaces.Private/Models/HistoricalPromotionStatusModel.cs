using System;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Model representing historical promotion status.
    /// </summary>
    public class HistoricalPromotionStatusModel
    {
        /// <summary>
        /// The date the promotion status was recorded on
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The total number of documents where the promote field is tagged as included
        /// </summary>
        public int TotalDocumentsIncluded { get; set; }

        /// <summary>
        /// The total number of documents where the promote field has not been set
        /// </summary>
        public int TotalDocumentsExcluded { get; set; }

        /// <summary>
        /// The total number of documents where the promote field is tagged as untagged
        /// </summary>
        public int TotalDocumentsUntagged { get; set; }
    }
}
