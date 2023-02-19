namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface IDocumentStatistics
    {
        long ForFolder(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
        long ForProduction(int workspaceArtifactId, int productionSetId);
        long ForSavedSearch(int workspaceArtifactId, int savedSearchId);
    }
}
