namespace Relativity.IntegrationPoints.Services
{
    public class CreateIntegrationPointRequest
    {
        public int WorkspaceArtifactId { get; set; }

        public IntegrationPointModel IntegrationPoint { get; set; }
    }
}
