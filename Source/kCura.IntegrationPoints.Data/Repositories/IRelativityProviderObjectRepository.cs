using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IRelativityProviderObjectRepository
	{
		/// <summary>
		/// Creates the object type in the given workspace
		/// </summary>
		/// <param name="parentArtifactTypeId">Parent Artifact Type id</param>
		/// <returns>The artifact type id of the newly created object</returns>
		int CreateObjectType(int parentArtifactTypeId);

		/// <summary>
		/// Creates the fields to the Source Workspace object type
		/// </summary>
		/// <param name="objectTypeId">The parent object's artifact type id</param>
		/// <param name="fieldGuids">The guids of the fields to create</param>
		/// <returns>A dictionary with the field guids as keys and the fields artifact ids as the values</returns>
		IDictionary<Guid, int> CreateObjectTypeFields(int objectTypeId, IEnumerable<Guid> fieldGuids);

		/// <summary>
		/// Creates the Source Workspace field on the Document object
		/// </summary>
		/// <param name="sourceWorkspaceObjectTypeId">The Source Workspace artifact type id</param>
		/// <returns>The artifact id of the newly created field</returns>
		int CreateFieldOnDocument(int sourceWorkspaceObjectTypeId);
	}
}
