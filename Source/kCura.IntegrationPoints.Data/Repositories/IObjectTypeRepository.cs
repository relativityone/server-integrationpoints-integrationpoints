using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling Object Types
    /// </summary>
    public interface IObjectTypeRepository
    {
        int CreateObjectType(Guid objectTypeGuid, string objectTypeName, int parentArtifactTypeId);

        /// <summary>
        /// Retrieves the Descriptor Artifact Type ID for the given object type GUID.
        /// </summary>
        /// <param name="objectTypeGuid">The GUID of the object type to find.</param>
        /// <returns>The Descriptor Artifact Type ID for the object type, <code>NULL</code> if not found.</returns>
        int RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid);

        /// <summary>
        /// Retrieves the Artifact ID for the given object type name.
        /// </summary>
        /// <param name="objectTypeName">The name of the object type to find.</param>
        /// <returns>The Artifact ID for the object type, <code>NULL</code> if not found.</returns>
        int? RetrieveObjectTypeArtifactId(string objectTypeName);

        ObjectTypeDTO GetObjectType(int typeId);
        int GetObjectTypeID(string objectTypeName);
        Dictionary<Guid, int> GetRdoGuidToArtifactIdMap();
    }
}
