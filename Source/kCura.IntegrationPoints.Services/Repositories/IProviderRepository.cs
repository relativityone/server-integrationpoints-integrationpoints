using System.Collections.Generic;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IProviderRepository
	{
		int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier);
		int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier);
		IList<ProviderModel> GetSourceProviders(int workspaceArtifactId);
		IList<ProviderModel> GetDesinationProviders(int workspaceArtifactId);
	}
}