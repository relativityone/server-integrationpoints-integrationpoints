namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to retrieve the current promotion status information from a specified workspace.
    /// </summary>
    public class CurrentPromotionStatusRequest
    {
        /// <summary>
        /// Gets or sets the artifact ID of the workspace to retrieve the information from.
        /// </summary>
        public int WorkspaceArtifactId { get; set; }
    }
}