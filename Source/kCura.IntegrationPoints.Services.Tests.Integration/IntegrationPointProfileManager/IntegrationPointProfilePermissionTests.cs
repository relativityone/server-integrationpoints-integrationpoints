using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	public class IntegrationPointProfilePermissionTests : KeplerServicePermissionsTestsBase
	{
		[Test]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateUserProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 144554, "ip_121").Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, 378660).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Wait());
		}

		[Test]
		public void MissingIntegrationPointViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, 181276).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Wait());
		}

		[Test]
		public void MissingIntegrationPointCreatePermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.AddSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Wait());
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 414974, "ip_266").Wait());
		}

		[Test]
		public void MissingIntegrationPointEditPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.EditSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Wait());
		}

		[Test]
		public void MissingIntegrationPointProfileViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 518923, "ip_584").Wait());
		}
	}
}