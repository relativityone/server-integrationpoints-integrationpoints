namespace kCura.IntegrationPoints.Data
{
	public interface IPermissionService 
	{
		/// <summary>
		/// Determines whether or not the current user has the "Allow Import" permission on the given workspace.
		/// </summary>
		/// <param name="workspaceId">The workspace to check the permission against.</param>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanImport(int workspaceId);

		/// <summary>
		/// Determines whether or not the current user has permission to "Edit" on documents.
		/// </summary>
		/// <param name="workspaceId">The workspace to check the permission against.</param>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanEditDocuments(int workspaceId);
	}
}
