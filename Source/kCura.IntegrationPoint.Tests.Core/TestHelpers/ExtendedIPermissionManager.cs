using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Group;
using Relativity.Services.Permission;
using Relativity.Services.User;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ExtendedIPermissionManager : IPermissionManager
	{
		private readonly ITestHelper _helper;
		private readonly ExecutionIdentity _identity;
		private Lazy<IPermissionManager> _managerWrapper;
		private IPermissionManager Manager => _managerWrapper.Value;

		public ExtendedIPermissionManager(ITestHelper helper, ExecutionIdentity identity)
		{
			_helper = helper;
			_identity = identity;
			_managerWrapper = new Lazy<IPermissionManager>(helper.CreateUserProxy<IPermissionManager>);
		}

		public async Task AddRemoveAdminGroupsAsync(GroupSelector groupSelector)
		{
			await Manager.AddRemoveAdminGroupsAsync(groupSelector).ConfigureAwait(false);
		}

		public async Task AddRemoveItemGroupsAsync(int workspaceArtifactId, int artifactId, GroupSelector groupSelector)
		{
			await Manager.AddRemoveItemGroupsAsync(workspaceArtifactId, artifactId, groupSelector).ConfigureAwait(false);
		}

		public async Task AddRemoveWorkspaceGroupsAsync(int workspaceArtifactId, GroupSelector groupSelector)
		{
			await Manager.AddRemoveWorkspaceGroupsAsync(workspaceArtifactId, groupSelector).ConfigureAwait(false);
		}

		public async Task<int> CreateSingleAsync(int workspaceArtifactId, global::Relativity.Services.Permission.Permission permissionDto)
		{
			return await Manager.CreateSingleAsync(workspaceArtifactId, permissionDto).ConfigureAwait(false);
		}

		public async Task DeleteSingleAsync(int workspaceArtifactId, int permissionId)
		{
			await Manager.DeleteSingleAsync(workspaceArtifactId, permissionId).ConfigureAwait(false);
		}

		private readonly object _obj = new object();

		public void Dispose()
		{
			lock (_obj)
			{
				Manager.Dispose();
				_managerWrapper = new Lazy<IPermissionManager>(_helper.CreateUserProxy<IPermissionManager>);
			}
		}

		public async Task<GroupPermissions> GetAdminGroupPermissionsAsync(global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetAdminGroupPermissionsAsync(group).ConfigureAwait(false);
		}

		public async Task<GroupSelector> GetAdminGroupSelectorAsync()
		{
			return await Manager.GetAdminGroupSelectorAsync().ConfigureAwait(false);
		}

		public async Task<List<global::Relativity.Services.User.UserRef>> GetAdminGroupUsersAsync(global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetAdminGroupUsersAsync(group).ConfigureAwait(false);
		}

		public async Task<GroupPermissions> GetItemGroupPermissionsAsync(int workspaceArtifactId, int artifactId, global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetItemGroupPermissionsAsync(workspaceArtifactId, artifactId, group).ConfigureAwait(false);
		}

		public async Task<GroupSelector> GetItemGroupSelectorAsync(int workspaceArtifactId, int artifactId)
		{
			return await Manager.GetItemGroupSelectorAsync(workspaceArtifactId, artifactId).ConfigureAwait(false);
		}

		public async Task<bool> IsUserInWorkspaceGroupAsync(int workspaceArtifactID, UserRef user, GroupRef group)
		{
			return await Manager.IsUserInWorkspaceGroupAsync(workspaceArtifactID, user, group);
		}

		public async Task<List<global::Relativity.Services.User.UserRef>> GetItemGroupUsersAsync(int workspaceArtifactId, int artifactId, global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetItemGroupUsersAsync(workspaceArtifactId, artifactId, group).ConfigureAwait(false);
		}

		public async Task<ItemLevelSecurity> GetItemLevelSecurityAsync(int workspaceArtifactId, int artifactId)
		{
			return await Manager.GetItemLevelSecurityAsync(workspaceArtifactId, artifactId).ConfigureAwait(false);
		}

		public async Task<Dictionary<int, ItemLevelSecurity>> GetItemLevelSecurityListAsync(int workspaceArtifactId, IEnumerable<int> artifactIDs)
		{
			return await Manager.GetItemLevelSecurityListAsync(workspaceArtifactId, artifactIDs).ConfigureAwait(false);
		}

		public async Task<List<PermissionValue>> GetPermissionSelectedAsync(int workspaceArtifactId, List<PermissionRef> permissions)
		{
			return await Manager.GetPermissionSelectedAsync(workspaceArtifactId, permissions).ConfigureAwait(false);
		}

		public async Task<List<PermissionValue>> GetPermissionSelectedAsync(int workspaceArtifactId, List<PermissionRef> permissions, int artifactId)
		{
			return await Manager.GetPermissionSelectedAsync(workspaceArtifactId, permissions, artifactId).ConfigureAwait(false);
		}

		public async Task<List<PermissionValue>> GetPermissionSelectedForGroupAsync(int workspaceArtifactId, List<PermissionRef> permissions, global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetPermissionSelectedForGroupAsync(workspaceArtifactId, permissions, group).ConfigureAwait(false);
		}

		public async Task<List<PermissionDetail>> GetAdminOperationPermissionSelectedListAsync(int workspaceArtifactID, List<int> permissionIds)
		{
			return await Manager.GetAdminOperationPermissionSelectedListAsync(workspaceArtifactID, permissionIds);
		}

		public async Task<List<PermissionValue>> GetPermissionSelectedForGroupAsync(int workspaceArtifactId, List<PermissionRef> permissions, global::Relativity.Services.Group.GroupRef group, int artifactId)
		{
			return await Manager.GetPermissionSelectedForGroupAsync(workspaceArtifactId, permissions, group, workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<Dictionary<int, List<PermissionValue>>> GetPermissionSelectedListAsync(int workspaceArtifactId, List<PermissionRef> permissions, IEnumerable<int> artifactIDs)
		{
			return await Manager.GetPermissionSelectedListAsync(workspaceArtifactId, permissions, artifactIDs).ConfigureAwait(false);
		}

		public async Task<GroupPermissions> GetWorkspaceGroupPermissionsAsync(int workspaceArtifactId, global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetWorkspaceGroupPermissionsAsync(workspaceArtifactId, group).ConfigureAwait(false);
		}

		public async Task<GroupSelector> GetWorkspaceGroupSelectorAsync(int workspaceArtifactId)
		{
			return await Manager.GetWorkspaceGroupSelectorAsync(workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<List<global::Relativity.Services.User.UserRef>> GetWorkspaceGroupUsersAsync(int workspaceArtifactId, global::Relativity.Services.Group.GroupRef group)
		{
			return await Manager.GetWorkspaceGroupUsersAsync(workspaceArtifactId, group).ConfigureAwait(false);
		}

		public async Task<PermissionQueryResultSet> QueryAsync(int workspaceArtifactId, global::Relativity.Services.Query query)
		{
			return await Manager.QueryAsync(workspaceArtifactId, query).ConfigureAwait(false);
		}

		public async Task<PermissionQueryResultSet> QueryAsync(int workspaceArtifactId, global::Relativity.Services.Query query, int length)
		{
			return await Manager.QueryAsync(workspaceArtifactId, query, length).ConfigureAwait(false);
		}

		public async Task<PermissionQueryResultSet> QuerySubsetAsync(int workspaceArtifactId, string queryToken, int start, int length)
		{
			return await Manager.QuerySubsetAsync(workspaceArtifactId, queryToken, start, length).ConfigureAwait(false);
		}

		public async Task<global::Relativity.Services.Permission.Permission> ReadSingleAsync(int workspaceArtifactId, int permissionId)
		{
			return await Manager.ReadSingleAsync(workspaceArtifactId, permissionId).ConfigureAwait(false);
		}

		public async Task SetAdminGroupPermissionsAsync(GroupPermissions groupPermissions)
		{
			await Manager.SetAdminGroupPermissionsAsync(groupPermissions).ConfigureAwait(false);
		}

		public async Task SetItemGroupPermissionsAsync(int workspaceArtifactId, GroupPermissions groupPermissions)
		{
			await Manager.SetItemGroupPermissionsAsync(workspaceArtifactId, groupPermissions).ConfigureAwait(false);
		}

		public async Task SetItemLevelSecurityAsync(int workspaceArtifactId, ItemLevelSecurity itemLevelSecurity)
		{
			await Manager.SetItemLevelSecurityAsync(workspaceArtifactId, itemLevelSecurity).ConfigureAwait(false);
		}

		public async Task SetPermissionSelectedForGroupAsync(int workspaceArtifactId, List<PermissionValue> permissionValues, global::Relativity.Services.Group.GroupRef group)
		{
			await Manager.SetPermissionSelectedForGroupAsync(workspaceArtifactId, permissionValues, group).ConfigureAwait(false);
		}

		public async Task SetPermissionSelectedForGroupAsync(int workspaceArtifactId, List<PermissionValue> permissionValues, global::Relativity.Services.Group.GroupRef group, int artifactId)
		{
			await Manager.SetPermissionSelectedForGroupAsync(workspaceArtifactId, permissionValues, group, artifactId).ConfigureAwait(false);
		}

		public async Task SetWorkspaceGroupPermissionsAsync(int workspaceArtifactId, GroupPermissions groupPermissions)
		{
			await Manager.SetWorkspaceGroupPermissionsAsync(workspaceArtifactId, groupPermissions).ConfigureAwait(false);
		}

		public async Task UpdateSingleAsync(int workspaceArtifactId, global::Relativity.Services.Permission.Permission permissionDto)
		{
			await Manager.UpdateSingleAsync(workspaceArtifactId, permissionDto).ConfigureAwait(false);
		}
	}
}