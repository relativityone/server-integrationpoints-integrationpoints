using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IDestinationWorkspaceRepository
	{
		/// <summary>
		/// Queries to see if a Destination Workspace RDO instance exists for the target workspace
		/// </summary>
		/// <returns>-1 if no instance exists, Artifact ID of instance otherwise</returns>
		int QueryDestinationWorkspaceRdoInstance();

		/// <summary>
		/// Creates an instance of a Destination Workspace RDO
		/// </summary>
		/// <param name="documentIds">The batched list of Document Artifact IDs to link to the new instance</param>
		/// <returns>The Artifact ID of the new instance</returns>
		int CreateDestinationWorkspaceRdoInstance(List<int> documentIds);

		/// <summary>
		/// Updates an instance of a Destination Workspace RDO, and adds more Document links to its Multi Object field
		/// </summary>
		/// <param name="documentIds"></param>
		/// <param name="destinationWorkspaceArtifactId">The batched list of Document Artifact IDs to link to the instance</param>
		/// <param name="existingMultiObjectLinks">A list of existing FieldValues the instance is linked to. We keep track of these so
		/// we do not have to keep querying for the fields</param>
		/// <param name="initialReadDone">Keeps track of if we have queried for the existing Multi Object field links yet</param>
		void UpdateDestinationWorkspaceRdoInstance(List<int> documentIds, int destinationWorkspaceArtifactId, ref FieldValueList<Artifact> existingMultiObjectLinks, bool initialReadDone = false);
	}
}
