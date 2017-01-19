namespace kCura.IntegrationPoints.Core.Contracts.Configuration
{
	public class SourceConfiguration
	{
		public int SavedSearchArtifactId { get; set; }
		public int SourceWorkspaceArtifactId { get; set; }
		public int TargetWorkspaceArtifactId { get; set; }
		public string SavedSearch { set; get; }
		public string SourceWorkspace { get; set; }
		public string TargetWorkspace { get; set; }
		public int? FederatedInstanceArtifactId { get; set; }
	}
}