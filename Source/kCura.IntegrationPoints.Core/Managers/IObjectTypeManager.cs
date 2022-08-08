using System;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IObjectTypeManager
    {
        /// <summary>
        /// Retrieves Descriptor Artifact Type Id for a given object
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace Artifact Id</param>
        /// <param name="objectTypeGuid">Object type guid</param>
        /// <returns>Artifact Type Id</returns>
        int RetrieveObjectTypeDescriptorArtifactTypeId(int workspaceArtifactId, Guid objectTypeGuid);
    }
}
