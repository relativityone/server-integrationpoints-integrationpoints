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

		/// <summary>
		/// Gets the Source Provider artifact id given a guid identifier
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
		/// <param name="sourceProviderGuidIdentifier">Guid identifier of Source Provider type</param>
		/// <returns>Artifact id of the Source Provider</returns>
		int GetArtifactIdFromSourceProviderTypeGuidIdentifier(int workspaceArtifactId, string sourceProviderGuidIdentifier);
	}
}