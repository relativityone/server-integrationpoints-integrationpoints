using System;
using System.Linq;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Core.Services
{
    /// <summary>
    /// Service handling verification and creation of data transfer job's folders
    /// </summary>
    public interface IDataTransferLocationService
    {
        /// <summary>
        /// Creates all necessary folders for all Integration Points Types
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace ID</param>
        void CreateForAllTypes(int workspaceArtifactId);

        /// <summary>
        /// Returns workspace fileshare root path
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace ID</param>
        /// <returns>Path as string</returns>
        string GetWorkspaceFileLocationRootPath(int workspaceArtifactId);

        /// <summary>
        /// Returns workspace root path for Integration Point Type
        /// </summary>
        /// <param name="integrationPointTypeIdentifier">Integration Point Type</param>
        /// <returns>Path as string</returns>
        string GetDefaultRelativeLocationFor(Guid integrationPointTypeIdentifier);

        /// <summary>
        /// Verifies and prepares all necessary folders
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace ID</param>
        /// <param name="path">Path to be used</param>
        /// <param name="providerType">provider type guid (Export/Import)</param>
        /// <returns>Verified path</returns>
        string VerifyAndPrepare(int workspaceArtifactId, string path, Guid providerType);

        /// <summary>
        /// Checks if given path is EDDS
        /// </summary>
        /// <param name="path">Path to be used</param>
        /// <returns>Boolean value</returns>
        bool IsEddsPath(string path);
    }
}
