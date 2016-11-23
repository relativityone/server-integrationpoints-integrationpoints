using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	[TestFixture]
	public class NoIntegrationPointViewPermissionTests : KeplerServiceMissingPermissionTests
	{
		protected override void CreatePermissionAndSetUsername()
		{
			var groupId = Group.CreateGroup($"group_{Identifier}");
			Username = $"test_{Identifier}@kcura.com";
			User.CreateUser("firstname", "lastname", Username, new List<int> {groupId});

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, groupId);
			var ipPerm = permissions.ObjectPermissions.FindPermission(@"Integration Point");
			ipPerm.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);
		}
	}
}