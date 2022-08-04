using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface ISourceJobManager
    {
        /// <summary>
        /// </summary>
        /// <param name="sourceWorkspaceArtifactId">The artifact id of the source workspace</param>
        /// <param name="destinationWorkspaceArtifactId">The artifact id of the destination workspace</param>
        /// <param name="jobHistoryArtifactId">The artifact id of the Job History rdo from the Source Workspace</param>
        /// <param name="sourceWorkspaceRdoInstanceArtifactId">
        ///     The artifact id of the instance of the parent Source Workspace rdo
        ///     to associate the new Source Job with
        /// </param>
        /// <returns></returns>
        SourceJobDTO CreateSourceJobDto(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, int jobHistoryArtifactId, int sourceWorkspaceRdoInstanceArtifactId);
    }
}