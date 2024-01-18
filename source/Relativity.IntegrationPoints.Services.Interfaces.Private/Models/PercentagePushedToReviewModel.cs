namespace Relativity.IntegrationPoints.Services
{
    public class PercentagePushedToReviewModel
    {
        /// <summary>
        /// The total number of documents in a workspace
        /// </summary>
        public int TotalDocuments { get; set; }

        /// <summary>
        /// The total number of documents that have been pushed to another workspace
        /// </summary>
        public int TotalDocumentsPushedToReview { get; set; }
    }
}
