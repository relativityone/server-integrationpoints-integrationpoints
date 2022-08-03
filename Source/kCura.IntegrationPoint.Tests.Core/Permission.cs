using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Permission
    {
        private static ITestHelper Helper => new TestHelper();

        public static GroupPermissions GetGroupPermissions(int workspaceID, int groupID)
        {
            return GetGroupPermissionsAsync(workspaceID, groupID).GetAwaiter().GetResult();
        }

        public static async Task<GroupPermissions> GetGroupPermissionsAsync(int workspaceID, int groupID)
        {
            var groupRef = new GroupRef(groupID);
            using (var proxy = Helper.CreateProxy<IPermissionManager>())
            {
                return await proxy.GetWorkspaceGroupPermissionsAsync(workspaceID, groupRef).ConfigureAwait(false);
            }
        }

        public static void SavePermission(int workspaceID, GroupPermissions permissions)
        {
            SavePermissionAsync(workspaceID, permissions).GetAwaiter().GetResult();
        }

        public static async Task SavePermissionAsync(int workspaceID, GroupPermissions permissions)
        {
            using (var proxy = Helper.CreateProxy<IPermissionManager>())
            {
                await proxy.SetWorkspaceGroupPermissionsAsync(workspaceID, permissions).ConfigureAwait(false);
            }
        }
    }
}