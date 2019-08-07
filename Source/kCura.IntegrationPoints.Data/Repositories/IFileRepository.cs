using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories.DTO;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		List<string> GetImagesLocationForProductionDocuments(int workspaceID, int productionID, int[] documentIDs);
		List<string> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs);
		List<FileDto> GetNativesForDocuments(int workspaceID, int[] documentIDs);
	}
}
