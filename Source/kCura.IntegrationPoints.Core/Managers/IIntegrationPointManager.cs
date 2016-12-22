using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Managers
{
	/// <summary>
	/// Manages Integration Point object operations.
	/// </summary>
	public interface IIntegrationPointManager
	{
		/// <summary>
		/// Retrieves an integration point.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance.</param>
		/// <param name="integrationPointArtifactId">Artifact id of the integration point instance.</param>
		/// <returns>An integration point object.</returns>
		IntegrationPointDTO Read(int workspaceArtifactId, int integrationPointArtifactId);

		/// <summary>
		/// Retrieves the integration point's source provider.
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance.</param>
		/// <param name="integrationPointDto">The integration point dto to check.</param>
		/// <returns>The SourceProvider for the integration point.</returns>
		Constants.SourceProvider GetSourceProvider(int workspaceArtifactId, IntegrationPointDTO integrationPointDto);
	}
}
