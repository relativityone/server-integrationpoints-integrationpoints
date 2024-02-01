namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to retrieve historical promotion status information from a specified workspace.
    /// </summary>
    public class HistoricalPromotionStatusRequest
    {
        /// <summary>
        /// The workspace to retrieve the information from
        /// </summary>
        public int WorkspaceArtifactId { get; set; }
    }
}
