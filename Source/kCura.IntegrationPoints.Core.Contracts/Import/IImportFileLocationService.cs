using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Contracts.Import
{
    public interface IImportFileLocationService
    {
        string ErrorFilePath(IntegrationPoint integrationPoint);
        LoadFileInfo LoadFileInfo(IntegrationPoint integrationPoint);
    }
}
