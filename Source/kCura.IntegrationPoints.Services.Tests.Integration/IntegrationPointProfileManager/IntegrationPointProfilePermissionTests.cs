using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	public class IntegrationPointProfilePermissionTests : KeplerServicePermissionsTestsBase
	{
		[IdentifiedTest("fadbee98-d1ba-4228-bd02-6e9ff1d2dca1")]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 144554, "ip_121").Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, 378660).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result);
		}

		[IdentifiedTest("181eef7c-bc01-491f-9254-6b643f004db3")]
		public void MissingIntegrationPointViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, 181276).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result);
		}

		[IdentifiedTest("b0f551b2-b904-4d43-b26f-3908dfbe9beb")]
		public void MissingIntegrationPointCreatePermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.AddSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileAsync(new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 414974, "ip_266").Result);
		}

		[IdentifiedTest("c3a43f0f-a541-4cf8-bb76-3d2af7487d89")]
		public void MissingIntegrationPointEditPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPointProfile);
			permissionsForRDO.EditSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.UpdateIntegrationPointProfileAsync(new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = WorkspaceArtifactId
			}).Result);
		}

		[IdentifiedTest("ad9f25e0-0468-420c-9d0e-df8f1aac1f89")]
		public void MissingIntegrationPointProfileViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.IntegrationPoint);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IIntegrationPointProfileManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.CreateIntegrationPointProfileFromIntegrationPointAsync(WorkspaceArtifactId, 518923, "ip_584").Result);
		}
	}
}