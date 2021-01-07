using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Interfaces.Group.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Group
	{
		private static ITestHelper Helper => new TestHelper();

		public static int CreateGroup(string name)
		{
			try
			{
				using (IGroupManager groupManager = Helper.CreateProxy<IGroupManager>())
				{
					GroupRequest request = new GroupRequest
					{
						Name = name
					};

					return groupManager.CreateAsync(request).GetAwaiter().GetResult()
						.ArtifactID;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($@"An error occurred while creating group {name}: {ex.Message}");
				throw;
			}
		}

		public static void DeleteGroup(int artifactId)
		{
			try
			{
				using (IGroupManager groupManager = Helper.CreateProxy<IGroupManager>())
				{
					groupManager.DeleteAsync(artifactId).GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($@"An error occurred while deleting group {artifactId}: {ex.Message}");
				throw;
			}
		}

		public static void AddGroupToWorkspace(int workspaceId, int groupId)
		{
			AddGroupToWorkspaceAsync(workspaceId, groupId).GetAwaiter().GetResult();
		}

		public static void RemoveGroupFromWorkspace(int workspaceId, int groupId)
		{
			RemoveGroupFromWorkspaceAsync(workspaceId, groupId).GetAwaiter().GetResult();
		}

		private static async Task AddGroupToWorkspaceAsync(int workspaceId, int groupId)
		{
			using (var proxy = Helper.CreateProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef>();
				groupSelector.EnabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}

		private static async Task RemoveGroupFromWorkspaceAsync(int workspaceId, int groupId)
		{
			using (var proxy = Helper.CreateProxy<IPermissionManager>())
			{
				GroupSelector groupSelector = await proxy.GetWorkspaceGroupSelectorAsync(workspaceId).ConfigureAwait(false);
				groupSelector.DisabledGroups = new List<GroupRef> { new GroupRef(groupId) };

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).ConfigureAwait(false);
			}
		}
	}
}
