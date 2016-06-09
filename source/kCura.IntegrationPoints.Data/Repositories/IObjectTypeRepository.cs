using System;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Object Types
	/// </summary>
	public interface IObjectTypeRepository
	{
		/// <summary>
		/// Retrieves the Descriptor Artifact Type ID for the given object type GUID.
		/// </summary>
		/// <param name="objectTypeGuid">The GUID of the object type to find.</param>
		/// <returns>The Descriptor Artifact Type ID for the object type, <code>NULL</code> if not found.</returns>
		int RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid);

		/// <summary>
		/// Retrieves the Descriptor Artifact Type ID for the given object type artifact ID.
		/// </summary>
		/// <param name="objectTypeArtifactId">The artifact ID of the object type to find.</param>
		/// <returns>The Descriptor Artifact Type ID for the object type, <code>NULL</code> if not found.</returns>
		int RetrieveObjectTypeDescriptorArtifactTypeId(int objectTypeArtifactId);

		/// <summary>
		/// Retrieves the Artifact ID for the given object type name.
		/// </summary>
		/// <param name="objectTypeName">The name of the object type to find.</param>
		/// <returns>The Artifact ID for the object type, <code>NULL</code> if not found.</returns>
		int? RetrieveObjectTypeArtifactId(string objectTypeName);

		/// <summary>
		/// Deletes the object type with the given artifact id
		/// </summary>
		/// <param name="artifactId"></param>
		void Delete(int artifactId);
	}
}