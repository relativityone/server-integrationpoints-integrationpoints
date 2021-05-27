using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
	public interface IImportFileLocationService
	{
		string ErrorFilePath(IntegrationPoint integrationPoint);
		string LoadFileFullPath(IntegrationPoint integrationPoint);
		System.IO.FileInfo LoadFileInfo(IntegrationPoint integrationPoint);
	}
}
