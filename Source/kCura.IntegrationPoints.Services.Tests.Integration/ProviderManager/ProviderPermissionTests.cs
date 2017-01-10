using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.ProviderManager
{
	public class ProviderPermissionTests : KeplerServicePermissionsTestsBase
	{
		[Test]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateUserProxy<IProviderManager>(UserModel.EmailAddress);

			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}

		[Test]
		public void MissingSourceProviderViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.SourceProvider);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IProviderManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}

		[Test]
		public void MissingDestinationProviderViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.DestinationProvider);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateUserProxy<IProviderManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}
	}
}