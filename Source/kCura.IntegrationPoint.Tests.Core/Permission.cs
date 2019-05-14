using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Permission
	{
		private static ITestHelper Helper => new TestHelper();

		public static GroupPermissions GetGroupPermissions(int workspaceId, int groupId)
		{
			return GetGroupPermissionsAsync(workspaceId, groupId).GetAwaiter().GetResult();
		}

		public static async Task<GroupPermissions> GetGroupPermissionsAsync(int workspaceId, int groupId)
		{
			var groupRef = new GroupRef(groupId);
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				return await proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).ConfigureAwait(false);
			}
		}

		public static void SavePermission(int workspaceId, GroupPermissions permissions)
		{
			SavePermissionAsync(workspaceId, permissions).GetAwaiter().GetResult();
		}

		public static async Task SavePermissionAsync(int workspaceId, GroupPermissions permissions)
		{
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				await proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, permissions).ConfigureAwait(false);
			}
		}

		public static async Task RemoveAddWorkspaceGroupAsync(int workspaceId, GroupSelector groupSelector)
		{
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				GroupSelector workspaceGroupSelector = proxy.GetWorkspaceGroupSelectorAsync(workspaceId).GetResultsWithoutContextSync();
				GroupSelector originalGroupSelector = proxy.GetAdminGroupSelectorAsync().GetResultsWithoutContextSync();
				originalGroupSelector.DisabledGroups = groupSelector.DisabledGroups;
				originalGroupSelector.EnabledGroups = groupSelector.EnabledGroups;
				originalGroupSelector.LastModified = workspaceGroupSelector.LastModified;

				await proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, originalGroupSelector).ConfigureAwait(false);
			}
		}
	}
}