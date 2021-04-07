﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Workspace;
using Relativity.Services.Permission;
using Relativity.Sync.Logging;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Authentication;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class ServiceFactoryForUserTests : SystemTest
	{
		private WorkspaceRef _workspace;
		private IUserService _userService;
		private ISyncServiceManager _servicesManager;

		[SetUp]
		public async Task SetUp()
		{
			_userService = RelativityFacade.Instance.Resolve<IUserService>();
			_servicesManager = new ServicesManagerStub();
			_workspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
		}

		private void SetUpUser(string userEmail)
		{
			IGroupService groupService = RelativityFacade.Instance.Resolve<IGroupService>();
			IClientService clientService = RelativityFacade.Instance.Resolve<IClientService>();
			IPermissionService permissionService = RelativityFacade.Instance.Resolve<IPermissionService>();

			Group group = groupService.Require(new Group {Name = "Test Group"});
			
			_userService.Require(new User
			{
				EmailAddress = userEmail,
				FirstName = "Test",
				LastName = "Test",
				Client = clientService.Get("Relativity"),
				Password = "Test1234!",
				Groups = new List<Artifact> { group }
			});

			permissionService.WorkspacePermissionService.AddWorkspaceToGroup(_workspace.ArtifactID, group.ArtifactID);
		}

		[IdentifiedTest("219adc9a-e1e4-4de8-bb46-c27fc05239e3")]
		public async Task UserShouldNotHavePermissionToWorkspace()
		{
			const string userEmail = "testuser@relativity.com";

			SetUpUser(userEmail);

			Mock<IUserContextConfiguration> userContextConfiguration = new Mock<IUserContextConfiguration>();
			userContextConfiguration.SetupGet(x => x.ExecutingUserId).Returns(_userService.GetByEmail(userEmail).ArtifactID);

			IAuthTokenGenerator authTokenGenerator = new OAuth2TokenGenerator(new OAuth2ClientFactory(_servicesManager, new EmptyLogger()),
				new TokenProviderFactoryFactory(), AppSettings.RelativityUrl, new EmptyLogger());
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
				permissionValues = await permissionManager.GetPermissionSelectedAsync(_workspace.ArtifactID, new List<PermissionRef> {permissionRef}).ConfigureAwait(false);
			}

			// ASSERT
			bool hasPermission = permissionValues.All(x => x.Selected);
			Assert.False(hasPermission);
		}
	}
}