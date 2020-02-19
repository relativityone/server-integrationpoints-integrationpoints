namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class AutomapSavedSearchRequest : AutomapRequest
	{
		public int SourceWorkspaceArtifactID { get; set; }
		public int SavedSearchArtifactID { get; set; }
	}
}