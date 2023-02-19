using System;
using kCura.IntegrationPoints.Data;

namespace Relativity.IntegrationPoints.Services.Helpers
{
    public class PermissionModel
    {
        public Guid ObjectTypeGuid { get; }

        public string ObjectTypeName { get; }

        public ArtifactPermission ArtifactPermission { get; }

        public PermissionModel(Guid objectTypeGuid, string objectTypeName, ArtifactPermission artifactPermission)
        {
            ObjectTypeGuid = objectTypeGuid;
            ObjectTypeName = objectTypeName;
            ArtifactPermission = artifactPermission;
        }
    }
}
