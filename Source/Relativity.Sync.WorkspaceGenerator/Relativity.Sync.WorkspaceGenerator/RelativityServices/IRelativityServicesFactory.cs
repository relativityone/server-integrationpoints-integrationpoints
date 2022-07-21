namespace Relativity.Sync.WorkspaceGenerator.RelativityServices
{
    public interface IRelativityServicesFactory
    {
        IWorkspaceService CreateWorkspaceService();
        ISavedSearchManager CreateSavedSearchManager();
        IProductionService CreateProductionService();
    }
}