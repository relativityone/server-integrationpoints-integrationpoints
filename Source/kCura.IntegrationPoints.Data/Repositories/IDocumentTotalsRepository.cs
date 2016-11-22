using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IDocumentTotalsRepository
	{
		int GetSavedSearchTotalDocsCount(int savedSearchId);

		int GetFolderTotalDocsCount(int folderId, int viewId, bool includeSubFoldersTotals);

		int GetProductionDocsCount(int productionSetId);

		int GetRdosCount(int artifactTypeId, int viewId);
	}
}
