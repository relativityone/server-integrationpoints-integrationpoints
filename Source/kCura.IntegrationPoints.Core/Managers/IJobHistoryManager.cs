using System.Threading.Tasks;

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
        /// Gets the stoppable job histories for a given Integration Point.
        /// </summary>
        /// <param name="workspaceArtifactId">The workspace artifact id.</param>
        /// <param name="integrationPointArtifactId">The parent Integration Point artifact id.</param>
        /// <returns>A StoppableJobCollection</returns>
        Models.StoppableJobHistoryCollection GetStoppableJobHistory(int workspaceArtifactId, int integrationPointArtifactId);

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
