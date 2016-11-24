using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	[TestFixture]
	public class NoWorkspacePermissionTests : KeplerServiceMissingPermissionTests
	{
		protected override void SetPermissions()
		{
		}
	}
}