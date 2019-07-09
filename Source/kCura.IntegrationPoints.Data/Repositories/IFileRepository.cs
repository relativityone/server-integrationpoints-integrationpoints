using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		List<string> GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs);
		List<string> GetImagesForDocuments(int workspaceID, int[] documentIDs);

	}
}
