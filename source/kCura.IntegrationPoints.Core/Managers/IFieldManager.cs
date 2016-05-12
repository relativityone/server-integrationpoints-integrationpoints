using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IFieldManager
	{
		/// <summary>
		/// Retrieves field artifact ids for the given field
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace to retrieve from</param>
		/// <param name="fieldGuids">Field guids to retrieve</param>
		/// <returns></returns>
		Dictionary<Guid, int> RetrieveFieldArtifactIds(int workspaceArtifactId, IEnumerable<Guid> fieldGuids);

		/// <summary>
		/// Retrieves the artifact view field id for the given field
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace to retrieve from</param>
		/// <param name="fieldArtifactId">The artifact id of the field</param>
		/// <returns>The artifact view field id if found, <code>NULL</code> otherwise</returns>
		int? RetrieveArtifactViewFieldId(int workspaceArtifactId, int fieldArtifactId);
	}


}
