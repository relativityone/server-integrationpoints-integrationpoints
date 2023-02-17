using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling the Source Workspace rdo and its functionality
    /// </summary>
    public interface ISourceWorkspaceRepository : IRelativityProviderObjectRepository
    {
        /// <summary>
        /// Retrieves the instance of Source Workspace for the given Source Workspace id
        /// </summary>
        /// <param name="sourceWorkspaceArtifactId">The artifact of the Workspace that initiated the push</param>
        /// <param name="federatedInstanceName">Name of the federated instance.</param>
        /// <param name="federatedInstanceArtifactId">Id of the federated instance.</param>
        /// <returns>A SourceWorkspaceDTO class representing the Source Workspace rdo, <code>NULL</code> if not found</returns>
        SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId, string federatedInstanceName, int? federatedInstanceArtifactId);

        /// <summary>
        /// Creates an instance of the Source Workspace rdo
        /// </summary>
        /// <param name="sourceWorkspaceDto">The Source Workspace to create</param>
        /// <returns>The artifact id of the newly created instance</returns>
        int Create(SourceWorkspaceDTO sourceWorkspaceDto);

        /// <summary>
        /// Updates the given Source Workspace rdo
        /// </summary>
        /// <param name="sourceWorkspaceDto">The Source Workspace to update</param>
        void Update(SourceWorkspaceDTO sourceWorkspaceDto);
    }
}
