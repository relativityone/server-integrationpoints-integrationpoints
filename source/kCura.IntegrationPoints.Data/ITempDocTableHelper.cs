using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data
{
	public interface ITempDocTableHelper
	{
		/// <summary>
		/// Creates a temporary scratch table on the EDDSResource table that stores the Artifact IDs of the Documents being exported
		/// </summary>
		/// <param name="artifactIds">The list of Artifact IDs being exported</param>
		void CreateTemporaryDocTable(List<int> artifactIds);

		/// <summary>
		/// Removes document from the temporary table if it has errored out
		/// </summary>
		/// <param name="docIdentifier">The document (by Control Number) to remove</param>
		void RemoveErrorDocument(string docIdentifier);

		/// <summary>
		/// Sets the table suffix variable for deleting of an errored Document
		/// </summary>
		/// <param name="tableSuffix">The unique suffix to set</param>
		void SetTableSuffix(string tableSuffix);

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
