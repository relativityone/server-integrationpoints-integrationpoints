using System;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public interface IRelativitySourceRdoObjectType
    {
        int CreateObjectType(int workspaceArtifactId, Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId);
    }
}
