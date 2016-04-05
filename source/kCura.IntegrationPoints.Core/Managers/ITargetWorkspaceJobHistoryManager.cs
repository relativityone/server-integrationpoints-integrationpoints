using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ITargetWorkspaceJobHistoryManager
	{
		TargetWorkspaceJobHistoryDTO InitializeWorkspace(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			int sourceWorkspaceArtifactTypeId,
			int sourceWorkspaceRDOInstanceArtifactId,
			int jobHistoryArtifactId);
	}
}