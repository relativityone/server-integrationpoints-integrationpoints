using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Services.Workspace;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.System.Stubs;
using Client = kCura.Relativity.Client.DTOs.Client;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ServiceFactoryForUserTests : SystemTest
	{
		private ServicesManagerStub _servicesManager;
		private ProvideServiceUrisStub _provideServiceUris;
		private WorkspaceRef _workspace;

		[SetUp]
		public async Task SetUp()
		{
			_servicesManager = new ServicesManagerStub();
			_provideServiceUris = new ProvideServiceUrisStub();
			_workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
		}

		private void SetUpGroup(string groupName)
		{
			Group group = GroupHelpers.GroupGetByName(Client, groupName);
			if (group == null)
			{
				CreateGroup(groupName);
				GroupHelpers.GroupGetByName(Client, groupName);
			}
		}

		private void CreateGroup(string name)
		{
			Group newGroup = new Group
			{
				Name = name,
				Users = new MultiUserFieldValueList(),
			};
			Client.Repositories.Group.Create(newGroup);
		}

		private void SetUpUser(string userName, string password, string groupName)
		{
			int userArtifactId = UserHelpers.FindUserArtifactID(Client, userName);

			User user;
			if (userArtifactId == 0)
			{
				Client client = Client.Repositories.Client.ReadSingle(_workspace.ArtifactID);
				user = UserHelpers.CreateUserWithPassword(Client, "Test", "Test", userName, client.Name, password);
			}
			else
			{
				user = Client.Repositories.User.ReadSingle(userArtifactId);
			}

			Group group = GroupHelpers.GroupGetByName(Client, groupName);
			GroupHelpers.GroupAddUserIfNotInGroup(Client, group, user);
		}

		private void AddGroupToWorkspace(string groupName)
		{
			using (IPermissionManager permissionManager = _servicesManager.CreateProxy<IPermissionManager>(ExecutionIdentity.System))
			{
				Group group = GroupHelpers.GroupGetOrCreateByName(Client, groupName);
				PermissionHelpers.AddGroupToWorkspace(permissionManager, _workspace.ArtifactID, group);
			}
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
			userContextConfiguration.SetupGet(x => x.ExecutingUserId).Returns(UserHelpers.FindUserArtifactID(Client, userName));

			IAuthTokenGenerator authTokenGenerator = new OAuth2TokenGenerator(new OAuth2ClientFactory(_servicesManager, new EmptyLogger()),
				new TokenProviderFactoryFactory(), _provideServiceUris, new EmptyLogger());
			PermissionRef permissionRef = new PermissionRef
			{
				ArtifactType = new ArtifactTypeIdentifier((int)ArtifactType.Batch),
				PermissionType = PermissionType.Edit
			};

			IDynamicProxyFactory dynamicProxyFactory = new DynamicProxyFactoryStub();
			ServiceFactoryForUser sut = new ServiceFactoryForUser(userContextConfiguration.Object, _servicesManager, authTokenGenerator, dynamicProxyFactory, new ServiceFactoryFactory());
			List<PermissionValue> permissionValues;
			using (IPermissionManager permissionManager = await sut.CreateProxyAsync<IPermissionManager>().ConfigureAwait(false))
			{
				// ACT
				permissionValues = await permissionManager.GetPermissionSelectedAsync(_workspace.ArtifactID, new List<PermissionRef> { permissionRef }).ConfigureAwait(false);
			}

			// ASSERT
			bool hasPermission = permissionValues.All(x => x.Selected);
			Assert.False(hasPermission);
		}
	}
}