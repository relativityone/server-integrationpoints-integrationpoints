using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling Workspace functionality
    /// </summary>
    public interface IWorkspaceRepository
    {
        /// <summary>
        /// Retrieves a workspace for the given workspace artifact id
        /// </summary>
        /// <param name="workspaceArtifactId">The artifact id of the workspace to retrieve</param>
        /// <param name="executionIdentity">Execution identify</param>
        /// <returns>A WorkspaceDTO object</returns>
        WorkspaceDTO Retrieve(int workspaceArtifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);

        /// <summary>
        /// Retrieves all workspaces
        /// </summary>
        /// <returns>WorkspaceDTO collection</returns>
        IEnumerable<WorkspaceDTO> RetrieveAll();

        /// <summary>
        /// Retrieves all active workspaces
        /// </summary>
        /// <returns>WorkspaceDTO collection</returns>
        IEnumerable<WorkspaceDTO> RetrieveAllActive();
    }
}
