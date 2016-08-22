using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	public interface ISourceProviderManager
	{
		/// <summary>
		/// Reads a Source Provider DTO
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
		/// <param name="sourceProviderArtifactId">Artifact id of the integration point instance</param>
		/// <returns>A Source Provider DTO</returns>
		SourceProviderDTO Read(int workspaceArtifactId, int sourceProviderArtifactId);
	}
}