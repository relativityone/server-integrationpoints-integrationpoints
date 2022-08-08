using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Workspace;

namespace kCura.IntegrationPoints.Data.Converters
{
    internal static class WorkspaceRefExtensions
    {
        public static WorkspaceDTO ToWorkspaceDTO(this WorkspaceRef workspaceRef)
        {
            if (workspaceRef == null)
            {
                return null;
            }

            return new WorkspaceDTO
            {
                Name = workspaceRef.Name,
                ArtifactId = workspaceRef.ArtifactID
            };
        }

        public static IEnumerable<WorkspaceDTO> ToWorkspaceDTOs(this IEnumerable<WorkspaceRef> workspaceRefs)
        {
            return workspaceRefs?.Select(ToWorkspaceDTO);
        }
    }
}
