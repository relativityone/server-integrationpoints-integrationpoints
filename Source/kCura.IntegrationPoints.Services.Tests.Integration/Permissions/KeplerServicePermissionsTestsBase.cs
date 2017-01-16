using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Permissions
{
	public abstract class KeplerServicePermissionsTestsBase : SourceProviderTemplate
	{
		protected UserModel UserModel;
		protected int GroupId;

		protected KeplerServicePermissionsTestsBase() : base($"permissions_{Utils.FormatedDateTimeNow}")
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();
			GroupId = Group.CreateGroup($"group_{Utils.FormatedDateTimeNow}");
			UserModel = User.CreateUser("firstname", "lastname", $"test_{Utils.FormatedDateTimeNow}@kcura.com", new List<int> {GroupId});
		}

		public override void TestTeardown()
		{
			base.TestTeardown();
			Group.DeleteGroup(GroupId);
			User.DeleteUser(UserModel.ArtifactId);
		}
	}
}