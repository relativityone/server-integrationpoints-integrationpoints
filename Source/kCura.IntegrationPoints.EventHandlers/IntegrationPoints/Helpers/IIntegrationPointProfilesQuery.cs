using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	internal interface IIntegrationPointProfilesQuery
	{
		Task<IEnumerable<IntegrationPointProfile>> GetAllProfilesAsync(int workspaceId);
		Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId);
		Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId);
		Task<IEnumerable<int>> GetSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactId, int syncDestinationProviderArtifactId);
		Task<IEnumerable<int>> GetNonSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactId, int syncDestinationProviderArtifactId);
	}
}