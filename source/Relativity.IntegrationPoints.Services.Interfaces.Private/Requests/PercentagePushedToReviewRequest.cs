namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to retrieve percentage pushed to review information from a specific workspace.
    /// </summary>
    public class PercentagePushedToReviewRequest
    {
        /// <summary>
        /// Gets or sets the workspace to retrieve the information from.
        /// </summary>
        public int WorkspaceArtifactId { get; set; }
    }
}