using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface IFieldManager
	{
		/// <summary>
		/// Retrieves the artifact view field id for the given field
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace to retrieve from</param>
		/// <param name="fieldArtifactId">The artifact id of the field</param>
		/// <returns>The artifact view field id if found, <code>NULL</code> otherwise</returns>
		int? RetrieveArtifactViewFieldId(int workspaceArtifactId, int fieldArtifactId);

		/// <summary>
		/// Retrieves the potential begin bates fields
		/// </summary>
		/// <returns>An array of ArtifactFieldDTOs</returns>
		ArtifactFieldDTO[] RetrieveBeginBatesFields(int workspaceArtifactId);

		/// <summary>
		/// Retrieves fields for Document object
		/// </summary>
		/// <param name="workspaceId">Artifact id of workspace</param>
		/// <param name="artifactTypeId">Artifact ID of the object type</param>
		/// <param name="fieldNames">The names of the fields to retrieve for the field artifact</param>
		/// <returns>An array of ArtifactDTO with populated fields for Document</returns>
		ArtifactDTO[] RetrieveFields(int workspaceId, int artifactTypeId, HashSet<string> fieldNames);
	}
}
