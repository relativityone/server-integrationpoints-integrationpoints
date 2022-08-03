using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IWorkspaceManager
    {
        IEnumerable<WorkspaceDTO> GetUserWorkspaces();
        IEnumerable<WorkspaceDTO> GetUserActiveWorkspaces();
        WorkspaceDTO RetrieveWorkspace(int workspaceArtifactId, ExecutionIdentity executionIdentity = ExecutionIdentity.CurrentUser);
        bool WorkspaceExists(int workspaceArtifactId);
        IEnumerable<WorkspaceDTO> GetUserAvailableDestinationWorkspaces(int sourceWorkspaceId);
    }
}