namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    public class DocumentTransferSettings
    {
        public int SavedSearchArtifactId { get; set; }
        public int SourceWorkspaceArtifactId { get; set; }
        public int TargetWorkspaceArtifactId { get; set; }
        public int? FederatedInstanceArtifactId { get; set; }
    }
}