using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Contracts.Import
{
	public interface IImportFileLocationService
	{
		string ErrorFilePath(IntegrationPoint integrationPoint);
		string LoadFileFullPath(IntegrationPoint integrationPoint);
		System.IO.FileInfo LoadFileInfo(IntegrationPoint integrationPoint);
	}
}
