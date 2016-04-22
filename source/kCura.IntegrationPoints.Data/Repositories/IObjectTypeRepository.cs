using System;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Object Types
	/// </summary>
	public interface IObjectTypeRepository
	{
		/// <summary>
		/// Retrieves the Descriptor Artifact Type id for the given object type guid
		/// </summary>
		/// <param name="objectTypeGuid">The Guid of the object type to find</param>
		/// <returns>The Descriptor Artifact Type id for the object type, <code>NULL</code> if not found</returns>
		int? RetrieveObjectTypeDescriptorArtifactTypeId(Guid objectTypeGuid);

		/// <summary>
		/// Deletes the object type with the given artifact id
		/// </summary>
		/// <param name="artifactId"></param>
		void Delete(int artifactId);
	}
}