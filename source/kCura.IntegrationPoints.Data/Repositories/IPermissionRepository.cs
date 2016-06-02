using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IPermissionRepository
	{
		/// <summary>
		/// Determines whether or not the current user has the "Allow Import" permission on the given workspace.
		/// </summary>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanImport();

		bool UserCanExport();

		/// <summary>
		/// Determines whether or not the current user has permission to "Edit" on documents.
		/// </summary>
		/// <returns>True if the user has the permission, false otherwise.</returns>
		bool UserCanEditDocuments();

		bool UserHasArtifactTypePermissions(Guid artifactTypeGuid, IEnumerable<ArtifactPermission> artifactPermissions);
		bool UserHasArtifactTypePermission(Guid artifactTypeGuid, ArtifactPermission artifactPermission);
		bool UserHasArtifactInstancePermission(Guid artifactTypeGuid, int artifactId, ArtifactPermission artifactPermission);
		bool UserHasArtifactInstancePermissions(Guid artifactTypeGuid, int artifactId,
			IEnumerable<ArtifactPermission> artifactPermissions);
		bool UserHasArtifactTypePermissions(int artifactTypeId, IEnumerable<ArtifactPermission> artifactPermissions);
		bool UserHasArtifactTypePermission(int artifactTypeId, ArtifactPermission artifactPermission);
		bool UserHasArtifactInstancePermission(int artifactTypeId, int artifactId, ArtifactPermission artifactPermission);
		bool UserHasPermissionToAccessWorkspace();


	}
}