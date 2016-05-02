using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	/// <summary>
	/// Manages Integration Point object operations
	/// </summary>
	public interface IIntegrationPointManager
	{
		/// <summary>
		/// Read integration point
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
		/// <param name="integrationPointArtifactId">Artifact id of the integration point instance</param>
		/// <returns>Integration point object</returns>
		IntegrationPointDTO Read(int workspaceArtifactId, int integrationPointArtifactId);
	}
}
