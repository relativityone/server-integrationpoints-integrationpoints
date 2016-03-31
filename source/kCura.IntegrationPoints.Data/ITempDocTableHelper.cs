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
		/// <param name="tableSuffix">The suffix that makes the table unique</param>
		void RemoveErrorDocument(string docIdentifier, string tableSuffix);
	}
}
