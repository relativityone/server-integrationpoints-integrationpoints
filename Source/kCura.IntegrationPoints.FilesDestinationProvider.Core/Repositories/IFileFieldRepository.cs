using Relativity.Services.FileField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories
{
	public interface IFileFieldRepository
	{
		DynamicFileResponse[] GetFilesForDynamicObjects(int workspaceID, int fileFieldArtifactID, int[] objectIDs);
	}
}
