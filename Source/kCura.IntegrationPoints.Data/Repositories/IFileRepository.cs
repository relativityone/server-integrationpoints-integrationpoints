using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		List<string> GetImagesLocationForProductionDocuments(int workspaceID, int productionID, int[] documentIDs);
		List<string> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs);

	}
}
