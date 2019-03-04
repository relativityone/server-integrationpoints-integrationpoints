using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.ServiceFactory;
using Group = kCura.Relativity.Client.DTOs.Group;
using TextCondition = kCura.Relativity.Client.TextCondition;
using TextConditionEnum = kCura.Relativity.Client.TextConditionEnum;
using User = kCura.Relativity.Client.DTOs.User;
using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;
using Workspace = kCura.Relativity.Client.DTOs.Workspace;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
	public sealed class ServiceFactoryForUserTests
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
	{
		private IRSAPIClient _client;
		private Workspace _workspace;

		// hardcoded settings until this story is not finished: REL-297309IN Create project for system tests
		private const string _RELATIVITY_ADMIN_USER_NAME = "relativity.admin@kcura.com";
		private const string _RELATIVITY_ADMIN_PASSWORD = "Test1234!";
		private const string _RELATIVITY_SERVICES_URL = "https://testvmaj10.kcura.corp/Relativity.Services/";
		private const string _RELATIVITY_REST_URL = "https://testvmaj10.kcura.corp/Relativity.Rest/api";

		[OneTimeSetUp]
		public void SuiteSetup()
		{
			_client = new RSAPIClient(new Uri(_RELATIVITY_SERVICES_URL), new UsernamePasswordCredentials(_RELATIVITY_ADMIN_USER_NAME, _RELATIVITY_ADMIN_PASSWORD));
			_workspace = CreateWorkspaceAsync().Result;
		}

		[OneTimeTearDown]
		public void SuiteTeardown()
		{
			DeleteWorkspace(_workspace.ArtifactID);
			_client?.Dispose();
		}

		[Test]
		public async Task UserShouldNotHavePermissionToWorkspace()
		{
			const string groupName = "Test Group";
			const string userName = "test@kcura.com";
			SetUpGroup(groupName);
			SetUpUser(userName, groupName);
			AddGroupToWorkspace(groupName);

			Mock<IServicesMgr> servicesManager = new Mock<IServicesMgr>();
			servicesManager.Setup(x => x.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser)).Returns(CreateUserProxy<IPermissionManager>(userName, _RELATIVITY_ADMIN_PASSWORD));
			ServiceFactoryForUser sut = new ServiceFactoryForUser(servicesManager.Object);
			IPermissionManager permissionManager = sut.CreateProxy<IPermissionManager>();
			PermissionRef permissionRef = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Batch),
				PermissionType = PermissionType.Edit
			};

			// ACT
			List<PermissionValue> permissionValues = await permissionManager.GetPermissionSelectedAsync(_workspace.ArtifactID, new List<PermissionRef>() { permissionRef }).ConfigureAwait(false);

			// ASSERT
			bool hasPermission = permissionValues.All(x => x.Selected);
			Assert.False(hasPermission);
		}

		private async Task<Workspace> CreateWorkspaceAsync()
		{
			string name = $"{Guid.NewGuid().ToString()}";
			Workspace newWorkspace = new Workspace() { Name = name };
			Workspace template = FindWorkspace("Relativity Starter Template");
			ProcessOperationResult createWorkspaceResult = _client.Repositories.Workspace.CreateAsync(template.ArtifactID, newWorkspace);

			if (!createWorkspaceResult.Success)
			{
				throw new ApplicationException($"Failed to create workspace '{newWorkspace.Name}': {createWorkspaceResult.Message}");
			}

			ProcessInformation processInfo;
			do
			{
				processInfo = _client.GetProcessState(_client.APIOptions, createWorkspaceResult.ProcessID);
				const int millisecondsDelay = 100;
				await Task.Delay(millisecondsDelay).ConfigureAwait(false);
			}
			while (processInfo.State == ProcessStateValue.Running || processInfo.State == ProcessStateValue.RunningWarning);

			if (processInfo.State != ProcessStateValue.Completed)
			{
				throw new ApplicationException($"Workspace creation did not completed successfuly: {processInfo.Message}");
			}

			return FindWorkspace(name);
		}

		private void DeleteWorkspace(int workspaceArtifactId)
		{
			_client.Repositories.Workspace.DeleteSingle(workspaceArtifactId);
		}

		private Workspace FindWorkspace(string name)
		{
			var workspaceNameCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, name);
			var query = new Query<Workspace>
			{
				Condition = workspaceNameCondition
			};
			query.Fields.Add(new FieldValue(WorkspaceFieldNames.Name));
			Workspace workspace = _client.Repositories.Workspace.Query(query).Results[0].Artifact;
			return workspace;
		}

		private T CreateUserProxy<T>(string username, string password) where T : IDisposable
		{
			var userCredential = new global::Relativity.Services.ServiceProxy.UsernamePasswordCredentials(username, password);
			ServiceFactorySettings userSettings = new ServiceFactorySettings(new Uri(_RELATIVITY_SERVICES_URL), new Uri(_RELATIVITY_REST_URL), userCredential);
			global::Relativity.Services.ServiceProxy.ServiceFactory userServiceFactory = new global::Relativity.Services.ServiceProxy.ServiceFactory(userSettings);
			return userServiceFactory.CreateProxy<T>();
		}

		private void SetUpGroup(string groupName)
		{
			Group group = GroupHelpers.GroupGetByName(_client, groupName);
			if (group == null)
			{
				CreateGroup(groupName);
				GroupHelpers.GroupGetByName(_client, groupName);
			}
		}

		private void CreateGroup(string name)
		{
			Group newGroup = new Group
			{
				Name = name,
				Users = new MultiUserFieldValueList(),
				Client = _workspace.Client
			};
			_client.Repositories.Group.Create(newGroup);
		}

		private void SetUpUser(string userName, string groupName)
		{
			int userArtifactId = UserHelpers.FindUserArtifactID(_client, userName);

			User user;
			if (userArtifactId == 0)
			{
				Client client = _client.Repositories.Client.ReadSingle(_workspace.Client.ArtifactID);
				user = UserHelpers.CreateUserWithPassword(_client, "Test", "Test", userName, client.Name, _RELATIVITY_ADMIN_PASSWORD);
			}
			else
			{
				user = _client.Repositories.User.ReadSingle(userArtifactId);
			}

			Group group = GroupHelpers.GroupGetByName(_client, groupName);
			GroupHelpers.GroupAddUserIfNotInGroup(_client, group, user);
		}

		private void AddGroupToWorkspace(string groupName)
		{
			using (IPermissionManager permissionManager = CreateUserProxy<IPermissionManager>(_RELATIVITY_ADMIN_USER_NAME, _RELATIVITY_ADMIN_PASSWORD))
			{
				Group group = GroupHelpers.GroupGetOrCreateByName(_client, groupName);
				PermissionHelpers.AddGroupToWorkspace(permissionManager, _workspace.ArtifactID, group);
			}
		}
	}
}