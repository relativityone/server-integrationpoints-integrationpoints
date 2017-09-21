namespace kCura.IntegrationPoints.Core.Services
{
    public interface IProcessingSourceLocationService
    {
        bool IsEnabled();
        bool IsProcessingSourceLocation(string path, int workspaceArtifactId);
    }
}