using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Permissions;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.ProviderManager
{
	public class ProviderPermissionTests : KeplerServicePermissionsTestsBase
	{
		[IdentifiedTest("8cd41525-6175-4c0e-a8bf-2a5030c8d34b")]
		public void MissingWorkspacePermission()
		{
			var client = Helper.CreateProxy<IProviderManager>(UserModel.EmailAddress);

			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}

		[IdentifiedTest("ac39ed62-53f0-4c84-96ae-292d76e173a0")]
		public void MissingSourceProviderViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.SourceProvider);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IProviderManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}

		[IdentifiedTest("d0249a07-67b7-4633-bfd7-00cd8365f128")]
		public void MissingDestinationProviderViewPermission()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForRDO = permissions.ObjectPermissions.FindPermission(ObjectTypes.DestinationProvider);
			permissionsForRDO.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);

			var client = Helper.CreateProxy<IProviderManager>(UserModel.EmailAddress);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviders(WorkspaceArtifactId).Result);
			PermissionsHelper.AssertPermissionErrorMessage(() => client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, Guid.NewGuid().ToString()).Result);
		}
	}
}