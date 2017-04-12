namespace kCura.IntegrationPoints.Services
{
	public class TotalItemsCountSearchModel :BaseModel
	{
		public string ExportType { get; set; }
		public int ArtifactTypeId { get; set; }
		public int FolderArtifactId { get; set; }
		public int ViewId { get; set; }
		public int ProductionId { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public int WorkspaceArtifactId { get; set; }
	}
}