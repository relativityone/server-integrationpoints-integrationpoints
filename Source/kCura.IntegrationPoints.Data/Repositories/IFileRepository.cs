using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		FileResponse[] GetNativesForSearch(int workspaceID, int[] documentIDs);

		FileResponse[] GetNativesForProduction(int workspaceID, int productionID, int[] documentIDs);

		ProductionDocumentImageResponse[] GetImagesForProductionDocuments(int workspaceID, int productionID, int[] documentIDs);

		DocumentImageResponse[] GetImagesForDocuments(int workspaceID, int[] documentIDs);

		FileResponse[] GetProducedImagesForDocument(int workspaceID, int documentID);

		ExportProductionDocumentImageResponse[] GetImagesForExport(int workspaceID, int[] productionIDs, int[] documentIDs);
	}
}
