namespace kCura.IntegrationPoints.Core.Services.Exporter
{
    public interface IFolderPathReaderFactory
    {
        IFolderPathReader Create(int workspaceArtifactID, bool useDynamicFolderPath);
    }
}