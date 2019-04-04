using Relativity.Services.FileField.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories
{
	public interface IFileFieldRepository
	{
		DynamicFileResponse[] GetFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs);
	}
}
