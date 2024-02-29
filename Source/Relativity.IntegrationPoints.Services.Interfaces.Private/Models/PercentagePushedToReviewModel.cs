namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a model containing the total number of documents in a workspace and the total number of documents pushed to another workspace.
    /// </summary>
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
