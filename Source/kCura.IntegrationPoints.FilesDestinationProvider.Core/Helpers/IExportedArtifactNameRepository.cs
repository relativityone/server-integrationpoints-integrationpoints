namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public interface IExportedArtifactNameRepository
    {
        string GetViewName(int workspaceId, int viewId);

        string GetProductionName(int workspaceId, int productionId);

        string GetSavedSearchName(int workspaceId, int savedSearchId);
    }
}