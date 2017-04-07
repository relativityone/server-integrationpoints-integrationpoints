using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	[WebService("Statistics Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IStatisticsManager : IKeplerService, IDisposable
	{
		Task<int> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);
		Task<int> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);
		Task<int> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);
		Task<int> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);
		Task<int> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

		Task<int> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId);
		Task<int> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);
		Task<int> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);
		Task<int> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);
		Task<int> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);


		Task<int> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
		Task<int> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
		Task<int> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
		Task<int> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
		Task<int> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
	}
}