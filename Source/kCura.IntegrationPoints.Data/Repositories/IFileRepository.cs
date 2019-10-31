using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.WinEDDS.Service.Export;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IFileRepository
	{
		ILookup<int, string> GetImagesLocationForProductionDocuments(int workspaceID, int productionID, int[] documentIDs, ISearchManager sm = null);

		ILookup<int, string> GetImagesLocationForDocuments(int workspaceID, int[] documentIDs, ISearchManager searchManager = null);

		List<FileDto> GetNativesForDocuments(int workspaceID, int[] documentIDs, ISearchManager sm = null);
	}
}
