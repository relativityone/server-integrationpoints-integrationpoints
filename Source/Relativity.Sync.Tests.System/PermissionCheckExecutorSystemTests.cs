using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
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
		private IGroupService _groupService;

		private User _user;
		private IUserService _userService;

		private int _destinationFolderArtifactId;

		private const string _TEST_USER_EMAIL = "testuser03@relativity.com";
		private const string _TEST_PASSWORD = "Test1234!";
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
			_destinationFolderArtifactId = await Rdos.CreateFolderInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID, _DESTINATION_FOLDER_NAME).ConfigureAwait(false);

			_groupService = RelativityFacade.Instance.Resolve<IGroupService>();
			string groupName = Guid.NewGuid().ToString();
			_group = _groupService.Create(new Group {Name = groupName});

			_userService = RelativityFacade.Instance.Resolve<IUserService>();
			_user = CreateAndSetUpUser(_TEST_USER_EMAIL, _TEST_PASSWORD, _group);

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
			ExecutionResult result = await executor.ExecuteAsync(_configurationStub, CompositeCancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);

			// Setup permissions for created user
			await SetUpPermissionsInWorkspaceAsync(_sourceWorkspace.ArtifactID, ObjectPermissionsForSource, SelectedAdminPermissionsForSource).ConfigureAwait(false);
			await SetUpPermissionsInWorkspaceAsync(_destinationWorkspace.ArtifactID, ObjectPermissionsForDestination, SelectedAdminPermissionsForDestination).ConfigureAwait(false);

			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IPermissionsCheckConfiguration>(_configurationStub);

			// Act-Assert
			Assert.DoesNotThrowAsync(() => syncJob.ExecuteAsync(CompositeCancellationToken.None));
		}

		[TearDown]
		public void TearDown()
		{
			_userService.Delete(_user.ArtifactID);
			_groupService.Delete(_group.ArtifactID);
		}

		private async Task SetUpPermissionsInWorkspaceAsync(int workspaceArtifactId, IEnumerable<ObjectPermissionSelection> objectPermissionSelections, IEnumerable<string> selectedAdminPermissions)
		{
			using (IPermissionManager permissionManager = ServiceFactory.CreateProxy<IPermissionManager>())
			{
				Services.Permission.GroupPermissions workspaceGroupPermissions = await permissionManager
					.GetWorkspaceGroupPermissionsAsync(workspaceArtifactId, GroupRef)
					.ConfigureAwait(false);

				foreach (ObjectPermissionSelection permissionConfig in objectPermissionSelections)
				{
					Services.Permission.ObjectPermission objectPermission = workspaceGroupPermissions.ObjectPermissions.Find(p => p.Name.Equals(permissionConfig.ObjectName, StringComparison.OrdinalIgnoreCase));
					objectPermission.AddSelected = permissionConfig.AddSelected;
					objectPermission.ViewSelected = permissionConfig.ViewSelected;
					objectPermission.EditSelected = permissionConfig.EditSelected;
				}

				foreach (string permissionName in selectedAdminPermissions)
				{
					Services.Permission.GenericPermission adminPermission = workspaceGroupPermissions.AdminPermissions
						.Find(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
					adminPermission.Selected = true;
				}

				await permissionManager.SetWorkspaceGroupPermissionsAsync(workspaceArtifactId, workspaceGroupPermissions).ConfigureAwait(false);
			}
		}

		private User CreateAndSetUpUser(string userEmail, string password, Group group)
		{
			IClientService clientService = RelativityFacade.Instance.Resolve<IClientService>();

			return _userService.Require(new User
			{
				EmailAddress = userEmail,
				FirstName = "Test",
				LastName = "Test",
				Client = clientService.Get("Relativity"),
				Password = password,
				Groups = new List<Artifact> { group }
			});
		}

		private async Task AddGroupToWorkspaceAsync(int workspaceId, Group group)
		{
			using (var proxy = ServiceFactory.CreateProxy<IPermissionManager>())
			{
				Services.Permission.GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef>();
				groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(group.ArtifactID) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}
	}
}