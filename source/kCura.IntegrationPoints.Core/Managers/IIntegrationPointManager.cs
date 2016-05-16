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
		/// <returns>The SourceProvider for the integration point</returns>
		Constants.SourceProvider GetSourceProvider(int workspaceArtifactId, IntegrationPointDTO integrationPointDto);

		/// <summary>
		/// Determines whether or not the current user has integration point permissions in the given workspace.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="integrationPointDto">The integration point dto to check</param>
		/// <param name="sourceProvider">The source provider for the integration point. If not supplied, the method will run queries to check.</param>
		/// <returns>A PermissionCheckDTO object</returns>
		PermissionCheckDTO UserHasPermissions(int workspaceArtifactId, IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null);
	}
}
