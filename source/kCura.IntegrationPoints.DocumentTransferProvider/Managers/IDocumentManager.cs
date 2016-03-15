using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

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
		Task<ArtifactDTO> RetrieveDocumentAsync(int documentId, ICollection<int> fieldIds);

		/// <summary>
		/// Retrieves multiple documents
		/// </summary>
		/// <param name="documentIds">The artifact ids of the documents to retrieve</param>
		/// <param name="fieldIds">The artifact Ids of the fields to retrieve</param>
		/// <returns>An array of document ArtifactDTOs</returns>
		Task<ArtifactDTO[]> RetrieveDocumentsAsync(IEnumerable<int> documentIds, HashSet<int> fieldIds);
	}
}