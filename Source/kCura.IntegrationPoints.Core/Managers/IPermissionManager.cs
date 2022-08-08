using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IPermissionManager
    {
        bool UserCanImport(int workspaceArtifactId);

        bool UserCanExport(int workspaceArtifactId);

        bool UserCanEditDocuments(int workspaceArtifactId);

        bool UserHasArtifactTypePermission(int workspaceArtifactId, int artifactTypeId, ArtifactPermission artifactPermission);

        bool UserHasArtifactTypePermission(int workspaceArtifactId, Guid artifactTypeGuid, ArtifactPermission artifactPermission);

        bool UserHasArtifactInstancePermission(int workspaceArtifactId, Guid artifactTypeGuid, int artifactId, ArtifactPermission artifactPermission);

        bool UserHasArtifactInstancePermission(int workspaceArtifactId, int artifactTypeId, int artifactId, ArtifactPermission artifactPermission);

        bool UserHasArtifactTypePermissions(int workspaceArtifactId, int artifactTypeId, IEnumerable<ArtifactPermission> artifactPermissions);

        bool UserHasPermissionToAccessWorkspace(int workspaceArtifactId);
    }
}
