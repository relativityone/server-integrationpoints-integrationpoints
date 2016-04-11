using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling the Source Workspace rdo and its functionality
	/// </summary>
	public interface ISourceWorkspaceRepository
	{
		/// <summary>
		/// Creates the Source Workspace object type
		/// </summary>
		/// <returns>The artifact type id of the new Source Workspace object type</returns>
		int CreateObjectType();

		/// <summary>
		/// Retrieves the instance of Source Workspace for the given Source Workspace id
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The artifact of the Workspace that initiated the push</param>
		/// <returns>A SourceWorkspaceDTO class representing the Source Workspace rdo, <code>NULL</code> if not found</returns>
		SourceWorkspaceDTO RetrieveForSourceWorkspaceId(int sourceWorkspaceArtifactId);

		/// <summary>
		/// Creates an instance of the Source Workspace rdo
		/// </summary>
		/// <param name="sourceWorkspaceArtifactTypeId">The Source Workspace artifact type id</param>
		/// <param name="sourceWorkspaceDto">The Source Workspace to create</param>
		/// <returns>The artifact id of the newly created instance</returns>
		int Create(int sourceWorkspaceArtifactTypeId, SourceWorkspaceDTO sourceWorkspaceDto);

		/// <summary>
		/// Creates the fields fo the Source Workspace object type
		/// </summary>
		/// <param name="sourceWorkspaceObjectTypeId">The Source Workspace artifact type id</param>
		/// <param name="fieldGuids">The guids of the fields to create</param>
		/// <returns>A dictionary with the field guids as keys and the fields artifact ids as the values</returns>
		IDictionary<Guid, int> CreateObjectTypeFields(int sourceWorkspaceObjectTypeId, IEnumerable<Guid> fieldGuids);

		/// <summary>
		/// Creates the Source Workspace field on the Document object
		/// </summary>
		/// <param name="sourceWorkspaceObjectTypeId">The Source Workspace artifact type id</param>
		/// <returns>The artifact id of the newly created field</returns>
		int CreateSourceWorkspaceFieldOnDocument(int sourceWorkspaceObjectTypeId);

		/// <summary>
		/// Updates the given Source Workspace rdo
		/// </summary>
		/// <param name="sourceWorkspaceDto">The Source Workspace to update</param>
		void Update(SourceWorkspaceDTO sourceWorkspaceDto);
	}
}