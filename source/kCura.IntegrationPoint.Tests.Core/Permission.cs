using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Extensions;
using Newtonsoft.Json;
using Relativity.Services.Group;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoint.Tests.Core
{
    using System;

    using kCura.IntegrationPoints.Data.Extensions;

    public static class Permission
	{
	    private static void IISReset()
	    {
			Process process = System.Diagnostics.Process.Start(@"C:\Windows\System32\iisreset.exe");
			process?.WaitForExit((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
		}
	
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
				proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, permissions).Wait(TimeSpan.FromSeconds(2));
			}
			IISReset();
		}

		public static void RemoveAddWorkspaceGroup(int workspaceId, GroupSelector groupSelector)
		{
			using (IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true))
			{
				proxy.AddRemoveWorkspaceGroupsAsync(workspaceId, groupSelector).Wait(TimeSpan.FromSeconds(2));
			}
			IISReset();
		}

		public static void SetMinimumRelativityProviderPermissions(int workspaceId, int groupId)
		{
			GroupRef groupRef = new GroupRef(groupId);

			using (IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName,
					SharedVariables.RelativityPassword, true, true))
			{
				GroupPermissions groupPermissions = proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).Result;

				SetObjectPermissions(groupPermissions, new List<string> { "Document", "Integration Point", "Job History", "Search" });
				SetAdminPermissions(groupPermissions, new List<string> { "Allow Import", "Allow Export" });
				SetTabVisibility(groupPermissions, new List<string> { "Documents", "Integration Points" });
				SetBrowserPermissions(groupPermissions, new List<string> { "Folders", "Advanced & Saved Searches" });

				proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, groupPermissions);
			}
			IISReset();
		}

        public static void SetGrouPermissions(int workspaceId, GroupPermissions groupPermissions)
        {
            using (
                IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(
                    SharedVariables.RelativityUserName,
                    SharedVariables.RelativityPassword,
                    true,
                    true))

            {
                proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, groupPermissions).Wait(TimeSpan.FromSeconds(5));
            } 
        }

        public static void SetPermissions(int workspaceId, int groupId, PermissionProperty permissionProperty)
        {
            GroupRef groupRef = new GroupRef(groupId);

            using (
                IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(
                    SharedVariables.RelativityUserName,
                    SharedVariables.RelativityPassword,
                    true,
                    true))
            {
                GroupPermissions groupPermissions = proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).GetResultsWithoutContextSync();
                
                SetObjectPermissions(groupPermissions, permissionProperty.Obj);
                SetAdminPermissions(groupPermissions, permissionProperty.Admin);
                SetTabVisibility(groupPermissions, permissionProperty.Tab);
                SetBrowserPermissions(groupPermissions, permissionProperty.Browser);

                proxy.SetWorkspaceGroupPermissionsAsync(workspaceId, groupPermissions).Wait(TimeSpan.FromSeconds(5));
            }
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

    public class PermissionProperty
    {
        public PermissionProperty()
        { }

        public  PermissionProperty(List<string> obj, List<string> admin, List<string> tab, List<string> browser)
        {
            Obj = obj;
            Admin = admin;
            Tab = tab;
            Browser = browser;
        }
        public List<string> Obj { get; set; }
        public List<string> Admin { get; set; }
        public List<string> Tab { get; set; }
        public List<string> Browser { get; set; }

    }
}
