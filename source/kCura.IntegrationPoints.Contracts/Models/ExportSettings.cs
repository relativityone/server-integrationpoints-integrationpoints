namespace kCura.IntegrationPoints.Contracts.Models
{
	public class ExportSettings
	{
		public int SourceWorkspaceArtifactId { set; get; }
		public int TargetWorkspaceArtifactId { get; set; }
		public string TargetWorkspace { get; set; }

		public string SourceWorkspace { get; set; }
	}

	public class ExportUsingSavedSearchSettings : ExportSettings
	{
		public int SavedSearchArtifactId { set; get; }
		public string SavedSearch { set; get; }
	}
}