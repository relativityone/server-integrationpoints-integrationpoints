using System.Collections.Generic;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISavedSearchRepository
	{
		/// <summary>
		/// Retrieves the next set of documents for the saved search
		/// </summary>
		/// <returns>The next set of documents for the saved search</returns>
		ArtifactDTO[] RetrieveNextDocuments();

		/// <summary>
		/// Checks to see if all documents have been retrieved
		/// </summary>
		/// <returns><code>TRUE</code> if more documents can be retrieved, <code>FALSE</code> otherwise</returns>
		bool AllDocumentsRetrieved();
	}
}