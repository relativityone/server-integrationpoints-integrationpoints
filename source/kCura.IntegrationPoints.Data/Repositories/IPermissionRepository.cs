namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IPermissionRepository
	{
		/// <summary>
		/// Determines whether or not the current user has the "Allow Import" permission on the given workspace.
		/// </summary>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanImport();

		/// <summary>
		/// Determines whether or not the current user has permission to "Edit" on documents.
		/// </summary>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanEditDocuments();

		/// <summary>
		/// Determines whether or not the current user has permissions to "View" an artifact.
		/// </summary>
		/// <param name="artifactTypeId">The artifact type id</param>
		/// <param name="artifactId">The artifact id of the instance to check permissions of</param>
		/// <returns>Returns <code>TRUE</code> if the user can view the artifact, <code>FALSE</code> otherwise.</returns>
		bool UserCanViewArtifact(int artifactTypeId, int artifactId);
	}
}