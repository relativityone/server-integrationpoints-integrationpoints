using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	/// <summary>
	/// Manages Integration Point object operations.
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

		/// <summary>
		/// Check to see if integration point is of a retriable type
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
		/// <param name="integrationPointDto">The integration point dto to check</param>
		/// <returns><code>TRUE</code> if the integration point is of a retriable type and <code>FALSE</code> otherwise</returns>
		bool IntegrationPointTypeIsRetriable(int workspaceArtifactId, IntegrationPointDTO integrationPointDto);
	}
}
