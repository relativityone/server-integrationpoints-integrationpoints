using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using kCura.Relativity.Client;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Web.Tests.Integration.Controllers
{
	[TestFixture]
	[Ignore("These tests are inconsistent - GetWorkspaceModels sometime returns 0 workspaces")]
	public class GetWorkspacesQueryTests : SourceProviderTemplate
	{
		private const string _userName = "gbadman@kcura.com";
		private const string _workspaceName = "GetWorkspacesQueryTests";

		private List<int> _groupIds;
		private List<int> _userIds;
		private List<int> _workspaceIds;
		private IHtmlSanitizerManager _htmlSanitizerManage;

		public GetWorkspacesQueryTests() : base(_workspaceName)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
		}

		public override void TestSetup()
		{
			_groupIds = new List<int>();
			_userIds = new List<int>();
			_workspaceIds = new List<int>();
			_htmlSanitizerManage = NSubstitute.Substitute.For<IHtmlSanitizerManager>();
			_htmlSanitizerManage.Sanitize(Arg.Any<string>()).Returns(new SanitizeResult() {CleanHTML = "Bla", HasErrors = false});
		}

		public override void TestTeardown()
		{
			Helper.RelativityUserName = SharedVariables.RelativityUserName;
			foreach (var artifactId in _userIds)
			{
				IntegrationPoint.Tests.Core.User.DeleteUser(artifactId);
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

			UserModel user = IntegrationPoint.Tests.Core.User.CreateUser("Gerron", "BadMan", _userName, new[] {groupId});
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient, _htmlSanitizerManage);
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

			UserModel user = IntegrationPoint.Tests.Core.User.CreateUser("Gerron", "BadMan", _userName, new[] {groupId, groupId2});
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient, _htmlSanitizerManage);
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

			UserModel user = IntegrationPoint.Tests.Core.User.CreateUser("Gerron", "BadMan", _userName, new[] {groupId, groupId2});
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId);
			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId2);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient, _htmlSanitizerManage);
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

			UserModel user = IntegrationPoint.Tests.Core.User.CreateUser("Gerron", "BadMan", _userName, new[] {groupId, groupId2});
			_userIds.Add(user.ArtifactId);

			Group.AddGroupToWorkspace(newWorkspaceArtifactId, groupId);
			Group.AddGroupToWorkspace(WorkspaceArtifactId, groupId2);

			Helper.RelativityUserName = _userName;
			IList<WorkspaceModel> results = null;
			using (IRSAPIClient rsapiClient = Helper.CreateUserProxy<IRSAPIClient>())
			{
				rsapiClient.APIOptions.WorkspaceID = -1;
				results = WorkspaceModel.GetWorkspaceModels(rsapiClient, _htmlSanitizerManage);
			}
			Assert.AreEqual(2, results.Count);
		}

		[Test]
		public void QueryWorkspaceModels_WithController_ExpectError()
		{
			//Arrange
			RsapiClientFactory rsapiClientFactory = Container.Resolve<RsapiClientFactory>();
			IWorkspaceService workspaceService = Substitute.For<IWorkspaceService>();
			workspaceService.GetWorkspaceID().Returns(WorkspaceArtifactId);
			WebClientFactory webClientFactory = new WebClientFactory(rsapiClientFactory, new[] { workspaceService });

			//Act
			WorkspaceFinderController workspaceFinderController = new WorkspaceFinderController(webClientFactory, null) {Request = new HttpRequestMessage()};
			workspaceFinderController.Request.SetConfiguration(new HttpConfiguration());
			HttpResponseMessage httpResponseMessage = workspaceFinderController.Get();

			string content = httpResponseMessage.Content.ReadAsStringAsync().Result;
			const string expectedResponseValue = "[]";

			//Assert
			Assert.AreEqual(HttpStatusCode.InternalServerError, httpResponseMessage.StatusCode);
			Assert.AreEqual(expectedResponseValue, content);
		}

		[Test]
		public void QueryWorkspaceModels_SavedSearchesWithController_Success()
		{
			//Arrange
			RsapiClientFactory rsapiClientFactory = Container.Resolve<RsapiClientFactory>();
			IWorkspaceService workspaceService = Substitute.For<IWorkspaceService>();
			workspaceService.GetWorkspaceID().Returns(WorkspaceArtifactId);
			WebClientFactory webClientFactory = new WebClientFactory(rsapiClientFactory, new [] { workspaceService });
			
			//Act
			WorkspaceFinderController workspaceFinderController = new WorkspaceFinderController(webClientFactory, _htmlSanitizerManage) { Request = new HttpRequestMessage() };
			workspaceFinderController.Request.SetConfiguration(new HttpConfiguration());
			HttpResponseMessage httpResponseMessage = workspaceFinderController.Get();
			string content = httpResponseMessage.Content.ReadAsStringAsync().Result;

			//Assert
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
			StringAssert.Contains(WorkspaceArtifactId.ToString(), content);
			StringAssert.Contains(Workspace.FindWorkspaceByName("New Case Template").ArtifactID.ToString(), content);
			StringAssert.Contains(Workspace.FindWorkspaceByName("kCura Starter Template").ArtifactID.ToString(), content);
		}
	}
}