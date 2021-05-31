using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface ISyncConfigurationService
	{
		Task<int?> TryGetResumedSyncConfigurationIdAsync(int workspaceId, int jobHistoryId);
	}
}
