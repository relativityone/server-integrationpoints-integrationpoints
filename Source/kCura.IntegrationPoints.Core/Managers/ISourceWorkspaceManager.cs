using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface ISourceWorkspaceManager
    {
        /// <summary>
        /// </summary>
        /// <param name="destinationWorkspaceArtifactId">The destination workspace artifact ID.</param>
        /// <param name="sourceWorkspaceArtifactId">The source workspace artifact ID.</param>
        /// <param name="federatedInstanceArtifactId">The federated instace artifact ID.</param>
        /// <returns></returns>
        SourceWorkspaceDTO CreateSourceWorkspaceDto(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, int? federatedInstanceArtifactId);
    }
}
