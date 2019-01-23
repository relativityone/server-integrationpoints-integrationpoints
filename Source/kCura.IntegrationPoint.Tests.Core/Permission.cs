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
			var groupRef = new GroupRef(groupId);
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				return proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).GetResultsWithoutContextSync();
			}
		}

		public static void SavePermission(int workspaceId, GroupPermissions permissions)
		{
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				Task.Run(async () => await proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, permissions)).Wait();
			}
		}

		public static void RemoveAddWorkspaceGroup(int workspaceId, GroupSelector groupSelector)
		{
			using (var proxy = Helper.CreateAdminProxy<IPermissionManager>())
			{
				GroupSelector workspaceGroupSelector = proxy.GetWorkspaceGroupSelectorAsync(workspaceId).GetResultsWithoutContextSync();
				GroupSelector originalGroupSelector = proxy.GetAdminGroupSelectorAsync().GetResultsWithoutContextSync();
				originalGroupSelector.DisabledGroups = groupSelector.DisabledGroups;
				originalGroupSelector.EnabledGroups = groupSelector.EnabledGroups;
				originalGroupSelector.LastModified = workspaceGroupSelector.LastModified;

				proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, originalGroupSelector).GetAwaiter().GetResult();
			}
		}
	}
}