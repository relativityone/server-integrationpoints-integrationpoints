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
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.System.Stub;
using TextCondition = kCura.Relativity.Client.TextCondition;
using TextConditionEnum = kCura.Relativity.Client.TextConditionEnum;
using User = kCura.Relativity.Client.DTOs.User;
using UsernamePasswordCredentials = kCura.Relativity.Client.UsernamePasswordCredentials;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ServiceFactoryForUserTests : IDisposable
	{
		private ServicesManagerStub _servicesManager;
		private ProvideServiceUrisStub _provideServiceUris;
		private IRSAPIClient _client;
		private Workspace _workspace;

		[OneTimeSetUp]
		public void SuiteSetup()
		{
			_servicesManager = new ServicesManagerStub();
			_provideServiceUris = new ProvideServiceUrisStub();
			_client = new RSAPIClient(AppSettings.RelativityServicesUrl, new UsernamePasswordCredentials(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword));
			_workspace = CreateWorkspaceAsync().Result;
		}

		[OneTimeTearDown]
		public void SuiteTeardown()
		{
			DeleteWorkspace(_workspace.ArtifactID);
			_client?.Dispose();
			_client = null;
		}
		
		[Test]
		public async Task UserShouldNotHavePermissionToWorkspace()
		{
			const string groupName = "Test Group";
			const string userName = "testuser@relativity.com";
			const string password = "Test1234!";
			SetUpGroup(groupName);
			SetUpUser(userName, password, groupName);
			AddGroupToWorkspace(groupName);

			Mock<IUserContextConfiguration> userContextConfiguration = new Mock<IUserContextConfiguration>();
			userContextConfiguration.SetupGet(x => x.ExecutingUserId).Returns(UserHelpers.FindUserArtifactID(_client, userName));

			IAuthTokenGenerator authTokenGenerator = new OAuth2TokenGenerator(new OAuth2ClientFactory(_servicesManager, new EmptyLogger()),
				new TokenProviderFactoryFactory(), _provideServiceUris, new EmptyLogger());
			PermissionRef permissionRef = new PermissionRef
			{
				ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Batch),
				PermissionType = PermissionType.Edit
			};

			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactoryStub();
			ServiceFactoryForUser sut = new ServiceFactoryForUser(userContextConfiguration.Object, _servicesManager, authTokenGenerator, dynamicProxyFactory);
			List<PermissionValue> permissionValues;
			using (IPermissionManager permissionManager = await sut.CreateProxyAsync<IPermissionManager>().ConfigureAwait(false))
			{
				// ACT
				permissionValues = await permissionManager.GetPermissionSelectedAsync(_workspace.ArtifactID, new List<PermissionRef> {permissionRef}).ConfigureAwait(false);
			}

			// ASSERT
			bool hasPermission = permissionValues.All(x => x.Selected);
			Assert.False(hasPermission);
		}

		private async Task<Workspace> CreateWorkspaceAsync()
		{
			string name = $"{Guid.NewGuid().ToString()}";
			Workspace newWorkspace = new Workspace {Name = name};
			Workspace template = FindWorkspace("Relativity Starter Template");
			ProcessOperationResult createWorkspaceResult = _client.Repositories.Workspace.CreateAsync(template.ArtifactID, newWorkspace);

			if (!createWorkspaceResult.Success)
			{
				throw new InvalidOperationException($"Failed to create workspace '{newWorkspace.Name}': {createWorkspaceResult.Message}");
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
				throw new InvalidOperationException($"Workspace creation did not completed successfuly: {processInfo.Message}");
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
			query.Fields.Add(new FieldValue("*"));
			Workspace workspace = _client.Repositories.Workspace.Query(query).Results[0].Artifact;
			return workspace;
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

		private void SetUpUser(string userName, string password, string groupName)
		{
			int userArtifactId = UserHelpers.FindUserArtifactID(_client, userName);

			User user;
			if (userArtifactId == 0)
			{
				Client client = _client.Repositories.Client.ReadSingle(_workspace.Client.ArtifactID);
				user = UserHelpers.CreateUserWithPassword(_client, "Test", "Test", userName, client.Name, password);
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
			using (IPermissionManager permissionManager = _servicesManager.CreateProxy<IPermissionManager>(ExecutionIdentity.System))
			{
				Group group = GroupHelpers.GroupGetOrCreateByName(_client, groupName);
				PermissionHelpers.AddGroupToWorkspace(permissionManager, _workspace.ArtifactID, group);
			}
		}

		public void Dispose()
		{
			_client?.Dispose();
		}
	}
}