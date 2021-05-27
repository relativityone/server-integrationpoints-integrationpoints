using System.IO;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Interfaces
{
	public interface IImportFileLocationService
	{
		string ErrorFilePath(int integrationPointArtifactId);
		string LoadFileFullPath(int integrationPointArtifactId);
		FileInfo LoadFileInfo(int integrationPointArtifactId);
	}
}
