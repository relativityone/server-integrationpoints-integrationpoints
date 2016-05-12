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
		/// Check to see if the integration point's source provider is the Relativity source provider
		/// </summary>
		/// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
		/// <param name="integrationPointDto">The integration point dto to check</param>
		/// <returns><code>TRUE</code> if the integration point's source provider is the Relativity source provider <code>FALSE</code> otherwise</returns>
		bool IntegrationPointSourceProviderIsRelativity(int workspaceArtifactId, IntegrationPointDTO integrationPointDto);

		/// <summary>
		/// Determines whether or not the current user has integration point permissions in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="integrationPointDto">The integration point dto to check</param>
		/// <returns><code>True</code> if the user has permissions, <code>false</code> otherwise.</returns>
		bool UserHasPermissions(int workspaceArtifactId, IntegrationPointDTO integrationPointDto);
	}
}
