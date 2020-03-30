using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Platform.Keywords.RSAPI;
using Relativity.Services;
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
				ObjectName = "Object Type",
				AddSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Search",
				AddSelected = true,
				ViewSelected = true
			},
			new ObjectPermissionSelection
			{
				ObjectName = "Document",
				AddSelected = true,
				ViewSelected = true,
				EditSelected = true
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

		public static IEnumerable<PermissionValue> DestinationPermissionValues => new []
		{
			new PermissionValue
			{
				ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Document),
				PermissionType = PermissionType.Add
			},
			new PermissionValue
			{
				ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Folder),
				PermissionType = PermissionType.Add
			},
			new PermissionValue
			{
				ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Document),
				PermissionType = PermissionType.Delete
			}
		};

		private GroupRef GroupRef => new GroupRef(_group.ArtifactID);

		[SetUp]
		public async Task SetUp()
		{
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
			_destinationFolderArtifactId = await Rdos.CreateFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID, _DESTINATION_FOLDER_NAME).ConfigureAwait(false);

			string groupName = Guid.NewGuid().ToString();
			const string userName = "testuser@relativity.com";
			const string password = "Test1234!";
			_group = CreateGroup(groupName);
			_user = CreateAndSetUpUser(userName, password, _group);
			await AddGroupToWorkspaceAsync(_sourceWorkspace.ArtifactID, _group).ConfigureAwait(false);
			await AddGroupToWorkspaceAsync(_destinationWorkspace.ArtifactID, _group).ConfigureAwait(false);
		}

		[IdentifiedTest("bdf02167-6dba-48e1-a2ba-2c14defeb581")]
		public async Task ItShouldValidatePermission()
		{
			_configurationStub = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID,
				DestinationFolderArtifactId = _destinationFolderArtifactId,
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID
			};

			await SetUpPermissionsInWorkspace(_sourceWorkspace.ArtifactID, ObjectPermissionsForSource, SelectedAdminPermissionsForSource).ConfigureAwait(false);
			await SetUpPermissionsInWorkspace(_destinationWorkspace.ArtifactID, ObjectPermissionsForDestination, SelectedAdminPermissionsForDestination).ConfigureAwait(false);
			await SetUpPermissionsForArtifactInWorkspace(_destinationWorkspace.ArtifactID, DestinationPermissionValues.ToList(), _destinationFolderArtifactId).ConfigureAwait(false);

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPermissionsCheckConfiguration>(_configurationStub);

			// Act-Assert
			Assert.DoesNotThrowAsync(async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false));
		}

		[TearDown]
		public void TearDown()
		{
			GroupHelpers.DeleteGroup(Client, _group);
			UserHelpers.DeleteUser(Client, _user);
		}

		private async Task SetUpPermissionsInWorkspace(int workspaceArtifactId, IEnumerable<ObjectPermissionSelection> objectPermissionSelections, IEnumerable<string> selectedAdminPermissions)
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

		private async Task SetUpPermissionsForArtifactInWorkspace(int workspaceArtifactId, List<PermissionValue> values, int objectArtifactId)
		{
			if (!values.IsNullOrEmpty())
			{
				using (IPermissionManager permissionManager = ServiceFactory.CreateProxy<IPermissionManager>())
				{
					await permissionManager
						.SetPermissionSelectedForGroupAsync(workspaceArtifactId, values, GroupRef, objectArtifactId)
						.ConfigureAwait(false);
				}
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