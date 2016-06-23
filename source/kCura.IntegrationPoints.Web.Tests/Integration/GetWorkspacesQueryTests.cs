using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using User = kCura.IntegrationPoint.Tests.Core.User;

namespace kCura.IntegrationPoints.Web.Tests.Integration
{
	[TestFixture]
	[Category("Integration Tests")]
	[Ignore("These tests are inconsistent - GetWorkspaceModels sometime returns 0 workspaces")]
	public class GetWorkspacesQueryTests : SingleWorkspaceTestTemplate
	{
		private const string _userName = "gbadman@kcura.com";
		private const string _workspaceName = "GetWorkspacesQueryTests";

		private List<int> _groupIds;
		private List<int> _userIds;
		private List<int> _workspaceIds;

		public GetWorkspacesQueryTests() :
			base(_workspaceName)
		{
		}

		[SetUp]
		public void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_workspaceIds = new List<int>();
		}

		[TearDown]
		public void TestTearDown()
		{
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
			foreach (var artifactId in _userIds)
			{
				User.DeleteUser(artifactId);
			}
			foreach (var artifactId in _groupIds)
			{
				Group.DeleteGroup(artifactId);
			}
			foreach (var artifactId in _workspaceIds)
			{
				Workspace.DeleteWorkspace(artifactId);
			}
		}

		[Test]
		public void GetWorkspaceModels_OnlyReturnPermittedWorkspace_UserInSingleGroup()
		{
			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			UserModel user = User.CreateUser("Gerron", "BadMan", _userName, new[] { groupId });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient);
			}
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void GetWorkspaceModels_OnlyReturnPermittedWorkspace_UserInMultipleGroups_OneGroupPermitted()
		{
			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			int groupId2 = Group.CreateGroup("krowten2.0(Extra Snitching)");
			_groupIds.Add(groupId2);

			UserModel user = User.CreateUser("Gerron", "BadMan", _userName, new[] { groupId, groupId2 });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient);
			}
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void GetWorkspaceModels_OnlyReturnPermittedWorkspace_UserInMultipleGroups_BothGroupsPermittted()
		{
			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			int groupId2 = Group.CreateGroup("krowten2.0(Extra Snitching)");
			_groupIds.Add(groupId2);

			UserModel user = User.CreateUser("Gerron", "BadMan", _userName, new[] { groupId, groupId2 });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);
			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId2);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient);
			}
			Assert.AreEqual(1, results.Count);
		}

		[Test]
		public void GetWorkspaceModels_OnlyReturnPermittedWorkspace_UserInMultipleGroups()
		{
			int newWorkspaceArtifactId = Workspace.CreateWorkspace("krowten only", _workspaceName);
			_workspaceIds.Add(newWorkspaceArtifactId);

			int groupId = Group.CreateGroup("krowten");
			_groupIds.Add(groupId);

			int groupId2 = Group.CreateGroup("krowten2.0(Extra Snitching)");
			_groupIds.Add(groupId2);

			UserModel user = User.CreateUser("Gerron", "BadMan", _userName, new[] { groupId, groupId2 });
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(newWorkspaceArtifactId, groupId);
			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId2);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient);
			}
			Assert.AreEqual(2, results.Count);
		}
	}
}