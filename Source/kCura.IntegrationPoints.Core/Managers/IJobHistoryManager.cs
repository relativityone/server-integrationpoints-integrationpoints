using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IJobHistoryManager
    {
        /// <summary>
        /// Gets last Job History artifact id for a given Integration Point
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace artifact id</param>
        /// <param name="integrationPointArtifactId">Integration Point artifact id</param>
        /// <returns>Artifact id of the Job History object</returns>
        int GetLastJobHistoryArtifactId(int workspaceArtifactId, int integrationPointArtifactId);

        /// <summary>
        /// Gets last finished Job History status for a given Integration Point
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace artifact id</param>
        /// <param name="integrationPointArtifactId">Integration Point artifact id</param>
        /// <returns>Status of the most recent finished Job History objects</returns>
        ChoiceRef GetLastJobHistoryStatus(int workspaceArtifactId, int integrationPointArtifactId);

        /// <summary>
        /// Gets last finished Job History for a given Integration Point
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace artifact id</param>
        /// <param name="integrationPointArtifactId">Integration Point artifact id</param>
        /// <returns>The most recent Job History object</returns>
        JobHistory GetLastJobHistory(int workspaceArtifactId, int integrationPointArtifactId);

        /// <summary>
        /// Set all associate job history errors to be expired
        /// </summary>
        /// <param name="workspaceArtifactID">The workspace artifact ID.</param>
        /// <param name="jobHistoryArtifactID">An artifact ID of the job history</param>
        void SetErrorStatusesToExpired(int workspaceArtifactID, int jobHistoryArtifactID);

        /// <summary>
        /// Set all associate job history errors to be expired
        /// </summary>
        /// <param name="workspaceArtifactID">The workspace artifact ID.</param>
        /// <param name="jobHistoryArtifactID">An artifact ID of the job history</param>
        Task SetErrorStatusesToExpiredAsync(int workspaceArtifactID, int jobHistoryArtifactID);
    }
}
