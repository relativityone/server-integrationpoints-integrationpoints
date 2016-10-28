using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IArtifactTreeService
	{
		JsTreeItemDTO GetArtifactTree(string artifactTypeName);
		JsTreeItemDTO GetArtifactTreeWithWorkspaceSet(string artifactTypeName,int workspaceId);
	}
}