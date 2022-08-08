using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.DTO;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling Documents
    /// </summary>
    public interface IDocumentRepository : IRepositoryWithMassUpdate
    {
        /// <summary>
        /// Retrieves multiple documents
        /// </summary>
        /// <param name="documentIds">The artifact ids of the documents to retrieve</param>
        /// <param name="fieldIds">The artifact Ids of the fields to retrieve</param>
        /// <returns>An array of document ArtifactDTOs</returns>
        Task<ArtifactDTO[]> RetrieveDocumentsAsync(
            IEnumerable<int> documentIds,
            HashSet<int> fieldIds);

        /// <summary>
        /// Retrieves multiple documents
        /// </summary>
        /// <param name="documentIds">The artifact ids of the documents to retrieve</param>
        /// <param name="fieldNames">The names of the fields to retrieve</param>
        /// <returns>An array of document ArtifactDTOs</returns>
        Task<ArtifactDTO[]> RetrieveDocumentsAsync(
            IEnumerable<int> documentIds,
            HashSet<string> fieldNames);

        /// <summary>
        /// Retrieve a single document via its identifier
        /// </summary>
        /// <param name="docIdentifierField">The field that is designated as the Document Identifier</param>
        /// <param name="docIdentifierValues">The collection of values of the given identifier that represents the Documents we want to retrieve</param>
        /// <returns>An array of ArtifactDTOs for the documents</returns>
        Task<int[]> RetrieveDocumentsAsync(
            string docIdentifierField,
            ICollection<string> docIdentifierValues);

        /// <summary>
        /// Retrieves the artifact ids of documents containing the specified identifier prefix
        /// </summary>
        /// <param name="documentIdentifierFieldName">The field name of the document identifier</param>
        /// <param name="identifierPrefix">The prefix for the document identifier</param>
        /// <returns>An array of artifact ids of the matching documents</returns>
        Task<int[]> RetrieveDocumentByIdentifierPrefixAsync(
            string documentIdentifierFieldName,
            string identifierPrefix);

        /// <summary>
        /// Initializes an export of documents returned by a Saved Search
        /// </summary>
        /// <param name="searchArtifactID"></param>
        /// <param name="artifactFieldIDs"></param>
        /// <param name="startAtRecord"></param>
        /// <returns>Initialization results that allow to perform an export process</returns>
        Task<ExportInitializationResultsDto> InitializeSearchExportAsync(
            int searchArtifactID,
            int[] artifactFieldIDs,
            int startAtRecord);

        /// <summary>
        /// Initializes an export of documents from a Production
        /// </summary>
        /// <param name="productionArtifactID"></param>
        /// <param name="artifactFieldIDs"></param>
        /// <param name="startAtRecord"></param>
        /// <returns>Initialization results that allow to perform an export process</returns>
        Task<ExportInitializationResultsDto> InitializeProductionExportAsync(
            int productionArtifactID,
            int[] artifactFieldIDs,
            int startAtRecord);

        /// <summary>
        /// Retrieves batch of data for the export starting at exportIndexID and of size lower or equal to resultsBlockSize
        /// </summary>
        /// <param name="initializationResults"></param>
        /// <param name="resultsBlockSize"></param>
        /// <param name="exportIndexID">List of exported documents</param>
        /// <returns>Initialization results that allow to perform an export process</returns>
        Task<IList<RelativityObjectSlimDto>> RetrieveResultsBlockFromExportAsync(
            ExportInitializationResultsDto initializationResults,
            int resultsBlockSize,
            int exportIndexID);

    }
}