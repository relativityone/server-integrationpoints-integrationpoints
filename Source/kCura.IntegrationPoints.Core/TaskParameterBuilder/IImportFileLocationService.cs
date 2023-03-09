using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core
{
    public interface IImportFileLocationService
    {
        string ErrorFilePath(int integrationPointArtifactId, string integrationPointName, string sourceConfiguration, ImportSettings destinationConfiguration);

        LoadFileInfo LoadFileInfo(string sourceConfiguration, ImportSettings destinationConfiguration);
    }
}
