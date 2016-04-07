using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Data.Factories
{
	public interface IRepositoryFactory
	{
		ISourceWorkspaceRepository GetSourceWorkspaceRepository(int workspaceArtifactId);
		ISourceWorkspaceJobHistoryRepository GetSourceWorkspaceJobHistoryRepository(int workspaceArtifactId);
		ITargetWorkspaceJobHistoryRepository GetTargetWorkspaceJobHistoryRepository(int workspaceArtifactId);
		IWorkspaceRepository GetWorkspaceRepository();
		IDestinationWorkspaceRepository GetDestinationWorkspaceRepository(int sourceWorkspaceArtifactId, int targetWorkspaceArtifactId);
		IJobHistoryRepository GetJobHistoryRepository();
	}
}