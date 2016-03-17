using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers
{
	public interface ISavedSearchManager
	{
		/// <summary>
		/// Retrieves the next set of documents for the saved search
		/// </summary>
		/// <returns>The next set of documents for the saved search</returns>
		ArtifactDTO[] RetrieveNext();

		/// <summary>
		/// Checks to see if all documents have been retrieved
		/// </summary>
		/// <returns><code>TRUE</code> if more documents can be retrieved, <code>FALSE</code> otherwise</returns>
		bool AllDocumentsRetrieved();
	}
}