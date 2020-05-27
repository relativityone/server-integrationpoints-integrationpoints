using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal class PermissionCheckExecutorSystemTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;
		private ConfigurationStub _configurationStub;
		private Group _group;
		private int _destinationFolderArtifactId;
		private User _user;

		private const string TestUserName = "testuser03@relativity.com";
		private const string TestPassword = "Test1234!";
		private const string _DESTINATION_FOLDER_NAME = "folderName";

		public static IEnumerable<ObjectPermissionSelection> ObjectPermissionsForSource => new[]
		{
			new ObjectPermissionSelection
			{
				ObjectName = "Job History",
				AddSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Object Type",
				AddSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Sync Batch",
				AddSelected = true,
				EditSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Sync Progress",
				AddSelected = true,
				EditSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Sync Configuration",
				EditSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Document",
				EditSelected = true,
				ViewSelected = true
			}
		};

		public static IEnumerable<ObjectPermissionSelection> ObjectPermissionsForDestination => new[]
		{
			new ObjectPermissionSelection
			{
				ObjectName = "Search",
				ViewSelected = true,
				AddSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Document",
				ViewSelected = true,
				AddSelected = true,
				EditSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Folder",
				ViewSelected = true,
				AddSelected = true
			},
			new ObjectPermissionSelection()
			{
				ObjectName = "Relativity Source Case",
				ViewSelected = true,
				AddSelected = true
			},
			new ObjectPermissionSelection()
			{
				ObjectName = "Relativity Source Job",
				ViewSelected = true,
				AddSelected = true
			}
		};

		public static IEnumerable<string> SelectedAdminPermissionsForSource => new[]
		{
			"Allow Export"
		};

		public static IEnumerable<string> SelectedAdminPermissionsForDestination => new[]
		{
			"Allow Import"
		};

		private GroupRef GroupRef => new GroupRef(_group.ArtifactID);

		[SetUp]
		public async Task SetUp()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.GetAwaiter().GetResult();
			_destinationWorkspace = destinationWorkspaceCreationTask.GetAwaiter().GetResult();
			_destinationFolderArtifactId = await Rdos.CreateFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID, _DESTINATION_FOLDER_NAME).ConfigureAwait(false);

			string groupName = Guid.NewGuid().ToString();
			_group = CreateGroup(groupName);
			_user = CreateAndSetUpUser(TestUserName, TestPassword, _group);
			await AddGroupToWorkspaceAsync(_sourceWorkspace.ArtifactID, _group).ConfigureAwait(false);
			await AddGroupToWorkspaceAsync(_destinationWorkspace.ArtifactID, _group).ConfigureAwait(false);
		}

		[IdentifiedTest("bdf02167-6dba-48e1-a2ba-2c14defeb581")]
		public async Task Execute_ShouldValidatePermission()
		{
			await Task.CompletedTask.ConfigureAwait(false);
			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				DestinationFolderArtifactId = _destinationFolderArtifactId,
				ExecutingUserId = _user.ArtifactID
			};

			// Initialize container
			IContainer container = ContainerHelper.Create(_configurationStub);

			// Create object types in destination (DestinationWorkspaceObjectTypesCreationExecutor is always executed in admin context)
			IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration> executor = container.Resolve<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>();
			ExecutionResult result = await executor.ExecuteAsync(_configurationStub, CancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);

			// Setup permissions for created user
			await SetUpPermissionsInWorkspaceAsync(_sourceWorkspace.ArtifactID, ObjectPermissionsForSource, SelectedAdminPermissionsForSource).ConfigureAwait(false);
			await SetUpPermissionsInWorkspaceAsync(_destinationWorkspace.ArtifactID, ObjectPermissionsForDestination, SelectedAdminPermissionsForDestination).ConfigureAwait(false);

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPermissionsCheckConfiguration>(_configurationStub);

			// Act-Assert
			Assert.DoesNotThrowAsync(() => syncJob.ExecuteAsync(CancellationToken.None));
		}

		[TearDown]
		public void TearDown()
		{
			GroupHelpers.DeleteGroup(Client, _group);
			UserHelpers.DeleteUser(Client, _user);
		}

		private async Task SetUpPermissionsInWorkspaceAsync(int workspaceArtifactId, IEnumerable<ObjectPermissionSelection> objectPermissionSelections, IEnumerable<string> selectedAdminPermissions)
		{
			using (IPermissionManager permissionManager = ServiceFactory.CreateProxy<IPermissionManager>())
			{
				GroupPermissions workspaceGroupPermissions = await permissionManager
					.GetWorkspaceGroupPermissionsAsync(workspaceArtifactId, GroupRef)
					.ConfigureAwait(false);

				foreach (ObjectPermissionSelection permissionConfig in objectPermissionSelections)
				{
					ObjectPermission objectPermission = workspaceGroupPermissions.ObjectPermissions.Find(p => p.Name.Equals(permissionConfig.ObjectName, StringComparison.OrdinalIgnoreCase));
					objectPermission.AddSelected = permissionConfig.AddSelected;
					objectPermission.ViewSelected = permissionConfig.ViewSelected;
					objectPermission.EditSelected = permissionConfig.EditSelected;
				}

				foreach (string permissionName in selectedAdminPermissions)
				{
					GenericPermission adminPermission = workspaceGroupPermissions.AdminPermissions
						.Find(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
					adminPermission.Selected = true;
				}

				await permissionManager.SetWorkspaceGroupPermissionsAsync(workspaceArtifactId, workspaceGroupPermissions).ConfigureAwait(false);
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

		private User CreateAndSetUpUser(string userName, string password, Group group)
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
			return user;
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