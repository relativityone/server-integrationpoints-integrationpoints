using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointTypeManager
{
	public class IntegrationPointTypePermissionTests : KeplerServicePermissionsTestsBase
	{
		[Test]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointTypeManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointTypes(WorkspaceArtifactId).Result);
		}

		[Test]
		public void MissingJobHistoryViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointType);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointTypeManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointTypes(WorkspaceArtifactId).Result);
		}
	}
}