namespace kCura.IntegrationPoints.Data.Statistics
{
	public interface IDocumentStatistics
	{
		int ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
		int ForProduction(int workspaceArtifactId, int productionSetId);
		int ForSavedSearch(int workspaceArtifactId, int savedSearchId);
	}
}