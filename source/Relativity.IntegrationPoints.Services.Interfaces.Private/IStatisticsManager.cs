using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
    [WebService("Statistics Manager")]
    [ServiceAudience(Audience.Private)]
    public interface IStatisticsManager : IKeplerService, IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="savedSearchId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetDocumentsTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="savedSearchId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="savedSearchId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesTotalForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="savedSearchId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="savedSearchId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesFileSizeForSavedSearchAsync(int workspaceArtifactId, int savedSearchId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="productionSetId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetDocumentsTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="productionSetId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="productionSetId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesTotalForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="productionSetId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="productionSetId"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesFileSizeForProductionAsync(int workspaceArtifactId, int productionSetId);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="folderId"></param>
        /// <param name="viewId"></param>
        /// <param name="includeSubFoldersTotals"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetDocumentsTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="folderId"></param>
        /// <param name="viewId"></param>
        /// <param name="includeSubFoldersTotals"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="folderId"></param>
        /// <param name="viewId"></param>
        /// <param name="includeSubFoldersTotals"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesTotalForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="folderId"></param>
        /// <param name="viewId"></param>
        /// <param name="includeSubFoldersTotals"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetImagesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);

        /// <summary>
        ///
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="folderId"></param>
        /// <param name="viewId"></param>
        /// <param name="includeSubFoldersTotals"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<long> GetNativesFileSizeForFolderAsync(int workspaceArtifactId, int folderId, int viewId, bool includeSubFoldersTotals);
    }
}
