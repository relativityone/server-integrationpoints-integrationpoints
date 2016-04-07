using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	public interface ITempDocTableHelper
	{
		/// <summary>
		/// Creates a temporary scratch table on the EDDSResource database that stores the Artifact IDs of the Documents being exported
		/// </summary>
		/// <param name="artifactIds">The list of Artifact IDs being exported</param>
		/// <param name="rdoTable">The type of RDO that the table is created for</param>
		void CreateTemporaryDocTable(List<int> artifactIds, ScratchTables rdoTable);

		/// <summary>
		/// Removes a document from the temporary table if it has errored out
		/// </summary>
		/// <param name="docIdentifier">The document to remove</param>
		void RemoveErrorDocument(string docIdentifier);

		/// <summary>
		/// Gets the list of Document Artifact IDs that were pushed
		/// </summary>
		/// <param name="rdoTable">The type of RDO, determines with of the scratch tables to operate on</param>
		/// <returns>List of Document IDs</returns>
		List<int> GetDocumentIdsFromTable(ScratchTables rdoTable);

		/// <summary>
		/// Deletes the temporary table after it is no longer needed
		/// </summary>
		/// <param name="rdoTable">The type of RDO, determines with of the scratch tables to operate on</param>
		void DeleteTable(ScratchTables rdoTable);

	}
}
