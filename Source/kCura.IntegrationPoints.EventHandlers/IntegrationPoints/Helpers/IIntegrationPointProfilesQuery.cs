using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	internal interface IIntegrationPointProfilesQuery
	{
		Task<(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds)> GetSyncAndNonSyncProfilesArtifactIdsAsync(int workspaceId);
	}
}