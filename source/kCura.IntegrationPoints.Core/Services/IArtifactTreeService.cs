using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IArtifactTreeService
	{
		TreeItemDTO GetArtifactTree(string artifactTypeName);
	}
}