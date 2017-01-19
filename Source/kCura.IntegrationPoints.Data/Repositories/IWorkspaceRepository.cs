using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

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
		/// <returns>A WorkspaceDTO object</returns>
		WorkspaceDTO Retrieve(int workspaceArtifactId);

		/// <summary>
		/// Retrieves all workspaces
		/// </summary>
		/// <returns>WorkspaceDTO collection</returns>
		IEnumerable<WorkspaceDTO> RetrieveAll();
	}
}