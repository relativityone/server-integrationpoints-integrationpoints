using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ISourceWorkspaceJobHistoryRepository
	{
		SourceWorkspaceJobHistoryDTO Retrieve(int jobHistoryArtifactId);
	}
}