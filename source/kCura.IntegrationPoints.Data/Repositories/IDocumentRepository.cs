using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Documents
	/// </summary>
	public interface IDocumentRepository
	{
		int WorkspaceArtifactId { get; set; }

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
		
		/// <summary>
		/// Retrieve a single document via its identifier
		/// </summary>
		/// <param name="docIdentifierField">The field that is designated as the Document Identifier</param>
		/// <param name="docIdentifierValue">The value of the given identifier that represents the Document we want to retrieve</param>
		/// <returns></returns>
		Task<ArtifactDTO> RetrieveDocumentAsync(string docIdentifierField, string docIdentifierValue);
	}
}