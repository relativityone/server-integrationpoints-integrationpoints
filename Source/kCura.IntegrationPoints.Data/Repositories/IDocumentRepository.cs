using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Responsible for handling Documents
	/// </summary>
	public interface IDocumentRepository
	{
		int WorkspaceArtifactId { get; set; }

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
		/// <param name="docIdentifierValues">The collection of values of the given identifier that represents the Documents we want to retrieve</param>
		/// <returns>An array of ArtifactDTOs for the documents</returns>
		Task<int[]> RetrieveDocumentsAsync(string docIdentifierField, ICollection<string> docIdentifierValues);

		/// <summary>
		/// Retrieves the artifact ids of documents containing the specified identifier prefix
		/// </summary>
		/// <param name="documentIdentifierFieldName">The field name of the document identifier</param>
		/// <param name="identifierPrefix">The prefix for the document identifier</param>
		/// <returns>An array of artifact ids of the matching documents</returns>
		Task<int[]> RetrieveDocumentByIdentifierPrefixAsync(string documentIdentifierFieldName, string identifierPrefix);
	}
}