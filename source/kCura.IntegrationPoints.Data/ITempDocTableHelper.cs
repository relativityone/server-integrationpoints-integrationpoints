using System.Collections.Generic;
using System.Data;

namespace kCura.IntegrationPoints.Data
{
	public interface ITempDocTableHelper
	{
		/// <summary>
		/// Creates a temporary scratch table on the EDDSResource database that stores the Artifact IDs of the Documents being exported
		/// </summary>
		/// <param name="artifactIds">The list of Artifact IDs being exported</param>
		/// <param name="tableName">The name of the temp table to be created </param>
		void AddArtifactIdsIntoTempTable(List<int> artifactIds, string tableName);

		/// <summary>
		/// Removes a document from the temporary table if it has errored out
		/// </summary>
		/// <param name="docIdentifier">The document to remove</param>
		void RemoveErrorDocument(string docIdentifier);

		/// <summary>
		/// Gets the list of Document Artifact IDs that were pushed
		/// </summary>
		/// <param name="tablePrefix">the scratch table's prefix to retrieve documents artifact ids from</param>
		/// <returns>List of Document IDs</returns>
		List<int> GetDocumentIdsFromTable(string tablePrefix);

		/// <summary>
		/// Deletes the temporary table after it is no longer needed
		/// </summary>
		/// <param name="tablePrefix">the scratch table's prefix to be deleted</param>
		void DeleteTable(string tablePrefix);

		IDataReader GetDocumentIdsDataReaderFromTable(string tablePrefix);

		string GetTempTableName(string tablePrefix);
	}
}
