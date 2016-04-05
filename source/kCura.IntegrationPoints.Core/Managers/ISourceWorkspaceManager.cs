using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ISourceWorkspaceManager
	{
		SourceWorkspaceDTO InititializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId);
	}
}