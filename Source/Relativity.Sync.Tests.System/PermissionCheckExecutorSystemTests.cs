using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using kCura.Relativity.Client.DTOs;
using kCura.WinEDDS.Service;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.API;
using Relativity.API.Foundation.Permissions;
using Relativity.Services.Group;
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
		private Group _group;
		private GroupRef GroupRef => new GroupRef(_group.ArtifactID);

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
			_group = CreateGroup(groupName);
			SetUpUser(userName, password, _group);
			await AddGroupToWorkspaceAsync(_sourceWorkspace.ArtifactID, _group).ConfigureAwait(false);
		}

		[Test]
		public async Task ItShouldValidatePermission()
		{
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);

			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID
			};

			IEnumerable<PermissionConfig> objectPermissionsForSource = new []
			{
				new PermissionConfig
				{
					ObjectName = "Job History",
					Add = true
				},
				new PermissionConfig
				{
					ObjectName = "Object Type",
					Add = true
				},
				new PermissionConfig
				{
					ObjectName = "Sync Batch",
					Add = true, Edit = true, View = true
				},
				new PermissionConfig
				{
					ObjectName = "Sync Progress",
					Add = true, Edit = true, View = true
				},
				new PermissionConfig
				{
					ObjectName = "Sync Configuration",
					Edit = true
				}
			};

			IEnumerable<PermissionConfig> objectPermissionsForDestination = new[]
			{
				new PermissionConfig(), 
			};
			await SetUpPermissions(_sourceWorkspace.ArtifactID, objectPermissionsForSource).ConfigureAwait(false);

			//ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPermissionsCheckConfiguration>(_configurationStub);

			// Act-Assert
			//Assert.DoesNotThrowAsync(async () =>
			//await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false));
		}

		private async Task SetUpPermissions(int sourceWorkspaceArtifactId, IEnumerable<PermissionConfig> permissionConfigs)
		{
			using (IPermissionManager permissionManager = ServiceFactory.CreateProxy<IPermissionManager>())
			{
				GroupPermissions workspaceGroupPermissions = await permissionManager
					.GetWorkspaceGroupPermissionsAsync(sourceWorkspaceArtifactId, GroupRef)
					.ConfigureAwait(false);

				foreach (PermissionConfig permissionConfig in permissionConfigs)
				{
					ObjectPermission permissionObject = workspaceGroupPermissions.ObjectPermissions.Find(permission => permission.Name.Equals(permissionConfig.ObjectName, StringComparison.OrdinalIgnoreCase));
					permissionObject.AddSelected = permissionConfig.Add;
					permissionObject.ViewSelected = permissionConfig.View;
					permissionObject.EditSelected = permissionConfig.Edit;
				}

				await permissionManager.SetWorkspaceGroupPermissionsAsync(sourceWorkspaceArtifactId, workspaceGroupPermissions).ConfigureAwait(false);
			}
		}

		private Group CreateGroup(string name)
		{
			Group newGroup = new Group
			{
				Name = name
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

		private async Task AddGroupToWorkspaceAsync(int workspaceId, Group group)
		{
			using (var proxy = ServiceFactory.CreateProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef>();
				groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(group.ArtifactID) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}
	}
}