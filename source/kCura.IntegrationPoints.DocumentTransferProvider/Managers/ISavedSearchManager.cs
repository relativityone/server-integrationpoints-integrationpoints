using kCura.IntegrationPoints.DocumentTransferProvider.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers
{
	public interface ISavedSearchManager
	{
		ArtifactDTO[] RetrieveNext();
		bool AllDocumentsRetrieved();
	}
}
