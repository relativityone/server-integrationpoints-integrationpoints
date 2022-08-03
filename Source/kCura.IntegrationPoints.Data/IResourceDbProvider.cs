namespace kCura.IntegrationPoints.Data
{
    public interface IResourceDbProvider
    {
        string GetSchemalessResourceDataBasePrepend(int workspaceID);
        string GetResourceDbPrepend(int workspaceID);
    }
}
