using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers
{
	public interface IIntegrationPointProfilesQuery
	{
		Task<(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds)> GetSyncAndNonSyncProfilesArtifactIdsAsync(int workspaceId);
	}
}