using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	internal interface IIntegrationPointProfilesQuery
	{
		Task<IEnumerable<IntegrationPointProfile>> GetAllProfilesAsync(int workspaceID);
		Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceID);
		Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceID);
		Task<IEnumerable<int>> GetSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID);
		Task<IEnumerable<int>> GetNonSyncProfilesAsync(IEnumerable<IntegrationPointProfile> profiles, int syncSourceProviderArtifactID, int syncDestinationProviderArtifactID);
	}
}