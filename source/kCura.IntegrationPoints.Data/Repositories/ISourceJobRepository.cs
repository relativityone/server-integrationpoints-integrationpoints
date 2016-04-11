using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Source Job rdos and their functionality
	/// </summary>
	public interface ISourceJobRepository
	{
		/// <summary>
		/// Creates the Source Job object type in the given workspace
		/// </summary>
		/// <param name="sourceWorkspaceArtifactTypeId">The Source Workspace artifact type id</param>
		/// <returns>The artifact type id of the newly created object</returns>
		int CreateObjectType(int sourceWorkspaceArtifactTypeId);

		/// <summary>
		/// Creates an instance of the Source Job rdo
		/// </summary>
		/// <param name="sourceJobArtifactTypeId">The artifact type id of the Source Job</param>
		/// <param name="sourceJobDto">The Source Job to create</param>
		/// <returns>The artifact id of the newly created rdo</returns>
		int Create(int sourceJobArtifactTypeId, SourceJobDTO sourceJobDto);

		/// <summary>
		/// Creates the Source Job multi-object field on the Document object
		/// </summary>
		/// <param name="sourceJobArtifactTypeId">The Source Job artifact type id</param>
		/// <returns></returns>
		int CreateSourceJobFieldOnDocument(int sourceJobArtifactTypeId);

		/// <summary>
		/// Creates the Source Job fields
		/// </summary>
		/// <param name="sourceJobArtifactTypeId">The Source Job artifact type id</param>
		/// <param name="fieldGuids">The guids of the fields to create</param>
		/// <returns>A dictionary with a key of the field guid and a value of field artifact id</returns>
		IDictionary<Guid, int> CreateObjectTypeFields(int sourceJobArtifactTypeId, IEnumerable<Guid> fieldGuids);
	}
}