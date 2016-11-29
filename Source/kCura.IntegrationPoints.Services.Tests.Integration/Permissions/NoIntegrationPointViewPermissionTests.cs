using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	[TestFixture]
	public class NoIntegrationPointViewPermissionTests : KeplerServiceMissingPermissionTests
	{
		protected override void SetPermissions()
		{
			Group.AddGroupToWorkspace(WorkspaceArtifactId, GroupId);

			var permissions = Permission.GetGroupPermissions(WorkspaceArtifactId, GroupId);
			var permissionsForIP = permissions.ObjectPermissions.FindPermission(@"Integration Point");
			permissionsForIP.ViewSelected = false;
			Permission.SavePermission(WorkspaceArtifactId, permissions);
		}
	}
}