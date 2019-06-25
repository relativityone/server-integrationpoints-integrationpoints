using System.Data;
using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		DataSet GetNativesForSearch(int workspaceID, int[] documentIDs);

		DataSet GetNativesForProduction(int workspaceID, int productionID, int[] documentIDs);

		DataSet GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs);

		DataSet GetImagesForDocuments(int workspaceID, int[] documentIDs);

		DataSet GetProducedImagesForDocument(int workspaceID, int documentID);

		DataSet GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs);
	}
}
