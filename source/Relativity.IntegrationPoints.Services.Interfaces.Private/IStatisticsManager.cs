using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    [WebService("Statistics Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IStatisticsManager : IKeplerService, IDisposable
    {
        Task<long> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        Task<long> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        Task<long> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        Task<long> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        Task<long> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        Task<long> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        Task<long> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        Task<long> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        Task<long> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        Task<long> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        Task<long> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        Task<long> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        Task<long> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        Task<long> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        Task<long> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
    }
}
