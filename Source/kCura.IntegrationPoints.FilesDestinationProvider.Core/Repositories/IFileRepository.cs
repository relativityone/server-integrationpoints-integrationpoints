using Relativity.Services.Interfaces.File.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories
{
	public interface IFileRepository
	{
		FileResponse[] GetNativesForSearchAsync(int workspaceID, int[] documentIDs);

		FileResponse[] GetNativesForProductionAsync(int workspaceID, int productionID, int[] documentIDs);

		ProductionDocumentImageResponse[] GetImagesForProductionDocumentsAsync(int workspaceID, int productionID, int[] documentIDs);

		DocumentImageResponse[] GetImagesForDocumentsAsync(int workspaceID, int[] documentIDs);

		FileResponse[] GetProducedImagesForDocumentAsync(int workspaceID, int documentID);

		ExportProductionDocumentImageResponse[] GetImagesForExportAsync(int workspaceID, int[] productionIDs, int[] documentIDs);
	}
}
