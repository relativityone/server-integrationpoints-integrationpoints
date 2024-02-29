namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to create an integration point in a specified workspace.
    /// </summary>
    public class CreateIntegrationPointRequest
    {
        /// <summary>
        /// Gets or sets the artifact ID of the workspace.
        /// </summary>
        public int WorkspaceArtifactId { get; set; }

        /// <summary>
        /// Gets or sets the integration point model containing the details of the integration point to be created.
        /// </summary>
        public IntegrationPointModel IntegrationPoint { get; set; }
    }
}