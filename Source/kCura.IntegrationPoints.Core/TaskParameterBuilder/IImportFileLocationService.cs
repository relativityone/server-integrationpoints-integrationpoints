using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core
{
    public interface IImportFileLocationService
    {
        string ErrorFilePath(int integrationPointArtifactId, string integrationPointName, string sourceConfiguration, DestinationConfiguration destinationConfiguration);

        LoadFileInfo LoadFileInfo(string sourceConfiguration, DestinationConfiguration destinationConfiguration);
    }
}
