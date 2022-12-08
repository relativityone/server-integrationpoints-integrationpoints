using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IProviderAccessor
    {
        int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier);
        int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier);
        IList<ProviderModel> GetSourceProviders(int workspaceArtifactId);
        IList<ProviderModel> GetDesinationProviders(int workspaceArtifactId);
    }
}
