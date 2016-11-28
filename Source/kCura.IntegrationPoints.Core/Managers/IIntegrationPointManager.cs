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

		/// <summary>
		/// Returns whether or not the current user can view errors.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <returns><code>TRUE</code> if the user can view errors, <code>FALSE</code> otherwise.</returns>
		PermissionCheckDTO UserHasPermissionToViewErrors(int workspaceArtifactId);

		/// <summary>
		/// Determines whether or not the current user has permissions to run an integration point in the given workspace. This only applies to the Relativity source provider.
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id.</param>
		/// <param name="integrationPointDto">The integration point dto to check.</param>
		/// <param name="sourceProvider">The source provider for the integration point. If not supplied, the method will run queries to check.</param>
		/// <returns>A PermissionCheckDTO object.</returns>
		PermissionCheckDTO UserHasPermissionToRunJob(int workspaceArtifactId, IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null);

		/// <summary>
		/// Checks to see if the user has the required permissions to save an Integration Point
		/// </summary>
		/// <param name="sourceWorkspaceArtifactId">The workspace artifact id that the Integration Point is being saved in.</param>
		/// <param name="integrationPointDto">The integration point to save</param>
		/// <param name="sourceProvider">States the Integration Point's source provider type. If not provided, the method will retrieve it internally.</param>
		/// <returns>A PermissionCheckDTO object.</returns>
		PermissionCheckDTO UserHasPermissionToSaveIntegrationPoint(int sourceWorkspaceArtifactId,
			IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null);

		/// <summary>
		/// Checks to see if the user has the required permissions to stop an Integration Point
		/// </summary>
		/// <param name="workspaceArtifactId">The workspace artifact id that the Integration Point is being stopped in.</param>
		/// <param name="integrationPointArtifactId">Artifact id of the integration point instance.</param>
		PermissionCheckDTO UserHasPermissionToStopJob(int workspaceArtifactId, int integrationPointArtifactId);
	}
}
