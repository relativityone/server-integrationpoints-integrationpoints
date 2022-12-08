using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Contracts.Import
{
    public interface IImportFileLocationService
    {
        string ErrorFilePath(int integrationPointArtifactId, string integrationPointName, string sourceConfiguration, string destinationConfiguration);

        LoadFileInfo LoadFileInfo(string sourceConfiguration, string destinationConfiguration);
    }
}
