using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.Helpers
{
	public class PermissionModel
	{
		public string ObjectTypeGuid { get; }
		public string ObjectTypeName { get; }
		public ArtifactPermission ArtifactPermission { get; }

		public PermissionModel(string objectTypeGuid, string objectTypeName, ArtifactPermission artifactPermission)
		{
			ObjectTypeGuid = objectTypeGuid;
			ObjectTypeName = objectTypeName;
			ArtifactPermission = artifactPermission;
		}
	}
}