using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IWorkspaceRepository
	{
		WorkspaceDTO Retrieve(int workspaceArtifactId);
	}
}