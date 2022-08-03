namespace kCura.IntegrationPoints.Data.Factories
{
    public interface IRelativityObjectManagerServiceFactory
    {
        IRelativityObjectManagerService Create(int workspaceArtifactId);
    }
}