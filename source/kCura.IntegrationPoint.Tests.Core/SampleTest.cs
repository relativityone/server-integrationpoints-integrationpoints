using kCura.IntegrationPoint.Tests.Core.Templates;

//using kCura.IntegrationPoints.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoint.Tests.Core
{
	[TestFixture]
	public class SampleTest : WorkspaceDependentTemplate
	{
		public SampleTest()
			: base("WorkspaceA", "WorkspaceB")
		{
		}

		//[Test]
		//[Explicit]
		//public void GoldFlow()
		//{
		//	int groupId = Group.CreateGroup("Smoke Test Group");
		//	bool addedGroupToWorkspace1 = Group.AddGroupToWorkspace(1119186, groupId);
		//	bool addedGroupToWorkspace2 = Group.AddGroupToWorkspace(1119028, groupId);

		//	bool assignedPermissions1 = Permission.SetMinimumRelativityProviderPermissions(1119186, groupId);
		//	bool assignedPermissions2 = Permission.SetMinimumRelativityProviderPermissions(1119028, groupId);

		//	UserModel user = User.CreateUser("New", "Test", "ntest@kcura.com", new[] { groupId });

		//	bool ranIntegrationPoint = false;
		//	try
		//	{
		//		// iisreset needed here?

		//		// replace hard coded values below
		//		string response = IntegrationPoint.CreateIntegrationPoint(Guid.NewGuid().ToString(),
		//			SourceWorkspaceArtifactId,
		//			TargetWorkspaceArtifactId,
		//			1039795,
		//			FieldOverlayBehavior.UseFieldSettings,
		//			ImportOverwriteMode.AppendOverlay,
		//			false,
		//			false,
		//			user);
		//		//IntegrationPointModel integrationPoint = JsonConvert.DeserializeObject<IntegrationPointModel>(response);
		//		//ranIntegrationPoint = IntegrationPoint.RunIntegrationPoint(SourceWorkspaceArtifactId, integrationPoint.ArtifactId, user);

		//	}
		//	catch
		//	{
		//		// TODO: handle this
		//	}

		//	bool deletedUser = User.DeleteUser(user.ArtifactId);
		//	bool deletedGroup = Group.DeleteGroup(groupId);

		//	Assert.IsTrue(addedGroupToWorkspace1);
		//	Assert.IsTrue(addedGroupToWorkspace2);
		//	Assert.IsTrue(assignedPermissions1);
		//	Assert.IsTrue(assignedPermissions2);
		//	//Assert.IsTrue(ranIntegrationPoint);
		//	Assert.IsTrue(deletedUser);
		//	Assert.IsTrue(deletedGroup);
		//}
	}
}