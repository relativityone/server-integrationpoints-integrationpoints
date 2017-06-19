using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IWorkspaceManager
	{
		IEnumerable<WorkspaceDTO> GetUserWorkspaces();
		IEnumerable<WorkspaceDTO> GetUserActiveWorkspaces();
		WorkspaceDTO RetrieveWorkspace(int workspaceArtifactId);
	    IEnumerable<WorkspaceDTO> GetUserAvailableDestinationWorkspaces(int sourceWorkspaceId);
	}
}