using System.Collections.Generic;
using kCura.IntegrationPoints.DocumentTransferProvider.Models;

namespace kCura.IntegrationPoints.DocumentTransferProvider.Managers
{
	public interface IDocumentManager
	{
		ArtifactDTO RetrieveDocument(int documentId, HashSet<int> fieldIds);
		ArtifactDTO[] RetrieveDocuments(IEnumerable<int> documentIds, HashSet<int> fieldIds);
	}
}