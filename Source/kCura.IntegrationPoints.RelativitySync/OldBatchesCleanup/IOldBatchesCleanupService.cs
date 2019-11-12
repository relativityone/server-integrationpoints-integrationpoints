using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.OldBatchesCleanup
{
	public interface IOldBatchesCleanupService
	{
		Task DeleteOldBatchesInWorkspaceAsync(int workspaceArtifactID);
	}
}