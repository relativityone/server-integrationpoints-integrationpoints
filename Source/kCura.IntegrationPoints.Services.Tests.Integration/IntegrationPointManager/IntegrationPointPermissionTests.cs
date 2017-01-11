using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	public class IntegrationPointPermissionTests : KeplerServicePermissionsTestsBase
	{
		[Test]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointFromProfileAsync(WorkspaceArtifactId, 707325, "ip_532").Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointAsync(WorkspaceArtifactId, 882287).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.RunIntegrationPointAsync(WorkspaceArtifactId, 906221).Result);
		}

		[Test]
		public void MissingIntegrationPointViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointAsync(WorkspaceArtifactId, 706302).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result);
		}

		[Test]
		public void MissingIntegrationPointCreatePermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permissionsForRDO.AddSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointFromProfileAsync(WorkspaceArtifactId, 209703, "ip_665").Result);
		}

		[Test]
		public void MissingIntegrationPointEditPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permissionsForRDO.EditSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.RunIntegrationPointAsync(WorkspaceArtifactId, 896501).Result);
		}

		[Test]
		public void MissingIntegrationPointProfileViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointFromProfileAsync(WorkspaceArtifactId, 334478, "ip_163").Result);
		}
	}
}