using System.Collections.Generic;
using Relativity.API;


namespace kCura.IntegrationPoints.Data
{
	public interface ITempDocTableHelper
	{
		/// <summary>
		/// Creates a temporary scratch table on the EDDSResource database that stores the Artifact IDs of the Documents being exported
		/// </summary>
		/// <param name="artifactIds">The list of Artifact IDs being exported</param>
		void CreateTemporaryDocTable(List<int> artifactIds);

		/// <summary>
		/// Removes a document from the temporary table if it has errored out
		/// </summary>
		/// <param name="docIdentifier">The document to remove</param>
		void RemoveErrorDocument(string docIdentifier);

		/// <summary>
		/// Gets the list of Document Artifact IDs that were pushed
		/// </summary>
		/// <returns>List of Document IDs</returns>
		List<int> GetDocumentIdsFromTable();

		/// <summary>
		/// Deletes the temporary table after it is no longer needed
		/// </summary>
		void DeleteTable();

	}
}
