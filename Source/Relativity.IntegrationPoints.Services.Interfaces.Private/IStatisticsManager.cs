using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Manager for retrieving statistics related to documents, natives, and images in Relativity workspaces.
    /// </summary>
    [WebService("Statistics Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IStatisticsManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Get the total number of documents for a saved search in the specified workspace.
        /// </summary>
        Task<long> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        /// Get the total number of natives for a saved search in the specified workspace.
        /// </summary>
        Task<long> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        /// Get the total number of images for a saved search in the specified workspace.
        /// </summary>
        Task<long> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        /// Get the total file size of images for a saved search in the specified workspace.
        /// </summary>
        Task<long> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        /// Get the total file size of natives for a saved search in the specified workspace.
        /// </summary>
        Task<long> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        /// Get the total number of documents for a production set in the specified workspace.
        /// </summary>
        Task<long> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        /// Get the total number of natives for a production set in the specified workspace.
        /// </summary>
        Task<long> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        /// Get the total number of images for a production set in the specified workspace.
        /// </summary>
        Task<long> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        /// Get the total file size of images for a production set in the specified workspace.
        /// </summary>
        Task<long> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        /// Get the total file size of natives for a production set in the specified workspace.
        /// </summary>
        Task<long> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        /// Get the total number of documents for a folder in the specified workspace.
        /// </summary>
        Task<long> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        /// Get the total number of natives for a folder in the specified workspace.
        /// </summary>
        Task<long> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        /// Get the total number of images for a folder in the specified workspace.
        /// </summary>
        Task<long> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        /// Get the total file size of images for a folder in the specified workspace.
        /// </summary>
        Task<long> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        /// Get the total file size of natives for a folder in the specified workspace.
        /// </summary>
        Task<long> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
    }
}
