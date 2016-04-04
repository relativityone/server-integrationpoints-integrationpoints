using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ISourceWorkspaceManager
	{
		SourceWorkspaceFieldMapDTO InititializeWorkspace(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId);
	}
}