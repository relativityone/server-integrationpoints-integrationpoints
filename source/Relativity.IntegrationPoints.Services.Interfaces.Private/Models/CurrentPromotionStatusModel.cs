namespace Relativity.IntegrationPoints.Services
{
    public class CurrentPromotionStatusModel
    {
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

        /// <summary>
        /// The total number of documents that have been pushed to another workspace
        /// </summary>
        public int TotalDocumentsPushedToReview { get; set; }
    }
}
