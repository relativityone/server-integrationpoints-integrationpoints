﻿using System;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.API;
using Relativity.API.Foundation.Permissions;
using Relativity.Services.Permission;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Transfer;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class PermissionCheckExecutorSystemTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;
		private ConfigurationStub _configurationStub;

		private const int _INTEGRATION_POINT_ARTIFACT_ID = 24234;

		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;

			string groupName = Guid.NewGuid().ToString();
			const string userName = "testuser@relativity.com";
			const string password = "Test1234!";
			Group group = CreateGroup(groupName);
			SetUpUser(userName, password, group);
			AddGroupToWorkspaces(group);
		}

		[Test]
		public async Task ItShouldValidatePermission()
		{
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);

			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				IntegrationPointArtifactId = _INTEGRATION_POINT_ARTIFACT_ID,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID
			};

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPermissionsCheckConfiguration>(_configurationStub);

		}

		private Group CreateGroup(string name)
		{
			Group newGroup = new Group
			{
				Name = name,
				Users = new MultiUserFieldValueList()
			};

			WriteResultSet<Group> result = Client.Repositories.Group.Create(newGroup);
			if (!result.Success)
			{
				throw new InvalidOperationException($"Cannot create group. Group name: {name}");
			}

			return GroupHelpers.GroupGetByName(Client, name);
		}

		private void SetUpUser(string userName, string password, Group group)
		{
			int userArtifactId = UserHelpers.FindUserArtifactID(Client, userName);

			User user;
			if (userArtifactId == 0)
			{
				user = UserHelpers.CreateUserWithPassword(Client, "Test", "Test", userName, "Relativity", password);
			}
			else
			{
				user = Client.Repositories.User.ReadSingle(userArtifactId);
			}

			GroupHelpers.GroupAddUserIfNotInGroup(Client, group, user);
		}

		private void AddGroupToWorkspaces(Group group)
		{
			using (IPermissionManager permissionManager =  ServiceFactory.CreateProxy<IPermissionManager>())
			{
				PermissionHelpers.AddGroupToWorkspace(permissionManager, _sourceWorkspace.ArtifactID, group);
				PermissionHelpers.AddGroupToWorkspace(permissionManager, _destinationWorkspace.ArtifactID, group);
			}
		}
	}
}