using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IArtifactTreeService
	{
		JsTreeItemDTO GetArtifactTreeWithWorkspaceSet(string artifactTypeName,int workspaceId = 0);
	}
}