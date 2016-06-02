using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;
using Newtonsoft.Json;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Permission
	{
		public static GroupPermissions GetGroupPermissions(int workspaceId, int groupId)
		{
			GroupRef groupRef = new GroupRef(groupId);
			using (IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				return proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).GetResultsWithoutContextSync();
			}
		}

		public static void SavePermission(int workspaceId, GroupPermissions permissions)
		{
			using (IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, permissions).Wait(TimeSpan.FromSeconds(10));
			}
			Process process = System.Diagnostics.Process.Start(@"C:\Windows\System32\iisreset.exe");
			process?.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
		}

		public static void RemoveAddWorkspaceGroup(int workspaceId, GroupSelector groupSelector)
		{
			using (IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).Wait(TimeSpan.FromSeconds(10));
			}
			Process process = Process.Start(@"C:\Windows\System32\iisreset.exe");
			process?.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
		}

		public static bool SetMinimumRelativityProviderPermissions(int workspaceId, int groupId, bool obj = true, bool admin = true, bool tab = true, bool browser = true)
		{
			GroupRef groupRef = new GroupRef(groupId);

			IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true);
			GroupPermissions groupPermissions = proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).Result;

		    if (obj)
		    {
		        SetObjectPermissions(groupPermissions, new List<string> {"Document", "Integration Point", "Job History", "Search"});
		    }

		    if (admin)
		    {
		        SetAdminPermissions(groupPermissions, new List<string> {"Allow Import", "Allow Export"});
		    }

		    if (tab)
		    {
		        SetTabVisibility(groupPermissions, new List<string> {"Documents", "Integration Points"});
		    }

		    if (browser)
		    {
		        SetBrowserPermissions(groupPermissions, new List<string> {"Folders", "Advanced & Saved Searches"});
		    }

			proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, groupPermissions);

			return true;
		}

		private static void SetObjectPermissions(GroupPermissions groupPermissions, List<string> permissionNames )
		{
			IEnumerable<int> indices = groupPermissions.ObjectPermissions.Select((value, index) => new { value, index })
					  .Where(x => permissionNames.Contains(x.value.Name))
					  .Select(x => x.index);

			foreach (int index in indices)
			{
				groupPermissions.ObjectPermissions[index].AddSelected = true;
				groupPermissions.ObjectPermissions[index].DeleteSelected = true;
				groupPermissions.ObjectPermissions[index].EditSelected = true;
				groupPermissions.ObjectPermissions[index].ViewSelected = true;
			}
		}

		public static void SetAdminPermissions(GroupPermissions groupPermissions, List<string> permissionNames)
		{
			IEnumerable<int> indices = groupPermissions.AdminPermissions.Select((value, index) => new { value, index })
					  .Where(x => permissionNames.Contains(x.value.Name))
					  .Select(x => x.index);

			foreach (int index in indices)
			{
				groupPermissions.AdminPermissions[index].Editable = true;
				groupPermissions.AdminPermissions[index].Selected = true;
			}
		}

		private static void SetBrowserPermissions(GroupPermissions groupPermissions, List<string> permissionNames)
		{
			IEnumerable<int> indices = groupPermissions.BrowserPermissions.Select((value, index) => new { value, index })
					  .Where(x => permissionNames.Contains(x.value.Name))
					  .Select(x => x.index);

			foreach (int index in indices)
			{
				groupPermissions.BrowserPermissions[index].Editable = true;
				groupPermissions.BrowserPermissions[index].Selected = true;
			}
		}

		private static void SetTabVisibility(GroupPermissions groupPermissions, List<string> permissionNames)
		{
			IEnumerable<int> indices = groupPermissions.TabVisibility.Select((value, index) => new { value, index })
					  .Where(x => permissionNames.Contains(x.value.Name))
					  .Select(x => x.index);

			foreach (int index in indices)
			{
				groupPermissions.TabVisibility[index].Editable = true;
				groupPermissions.TabVisibility[index].Selected = true;
				foreach (GenericPermission child in groupPermissions.TabVisibility[index].Children)
				{
					child.Editable = true;
					child.Selected = true;
				}
			}
		}
	}
}
