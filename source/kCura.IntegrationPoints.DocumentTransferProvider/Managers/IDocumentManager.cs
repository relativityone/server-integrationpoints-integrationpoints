using System.Collections.Generic;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers
{
	public interface IDocumentManager
	{
		/// <summary>
		/// Retrieves a single document
		/// </summary>
		/// <param name="documentId">The artifact id of the document to retrieve</param>
		/// <param name="fieldIds">The artifact Ids of the fields to retrieve</param>
		/// <returns>An ArtifactDTO for the document</returns>
		ArtifactDTO RetrieveDocument(int documentId, HashSet<int> fieldIds);

		/// <summary>
		/// Retrieves multiple documents
		/// </summary>
		/// <param name="documentIds">The artifact ids of the documents to retrieve</param>
		/// <param name="fieldIds">The artifact Ids of the fields to retrieve</param>
		/// <returns>An ArtifactDTO for the document</returns>
		ArtifactDTO[] RetrieveDocuments(IEnumerable<int> documentIds, HashSet<int> fieldIds);
	}
}