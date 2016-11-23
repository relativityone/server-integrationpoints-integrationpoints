using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	[TestFixture]
	public class NoWorkspacePermissionTests : KeplerServiceMissingPermissionTests
	{
		protected override void CreatePermissionAndSetUsername()
		{
			var groupId = Group.CreateGroup($"group_{Identifier}");
			Username = $"test_{Identifier}@kcura.com";
			User.CreateUser("firstname", "lastname", Username, new List<int> {groupId});
		}
	}
}