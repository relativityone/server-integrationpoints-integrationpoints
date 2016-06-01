﻿using System.Collections.Generic;
using System.Linq;
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
			string parameter1 = $"{{workspaceArtifactID:{workspaceId},group:{JsonConvert.SerializeObject(groupRef)}}}";

			string response1 = Rest.PostRequestAsJson("api/Relativity.Services.Permission.IPermissionModule/Permission Manager/GetWorkspaceGroupPermissionsAsync",
				false, parameter1);
			GroupPermissions groupPermissions = JsonConvert.DeserializeObject<GroupPermissions>(response1);

			return groupPermissions;
		}

		public static bool SetMinimumRelativityProviderPermissions(int workspaceId, int groupId)
		{
			GroupRef groupRef = new GroupRef(groupId);

			IPermissionManager proxy = Kepler.CreateProxy<IPermissionManager>(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword, true, true);
			GroupPermissions groupPermissions = proxy.GetWorkspaceGroupPermissionsAsync(workspaceId, groupRef).Result;

			SetObjectPermissions(groupPermissions, new List<string> {"Document", "Integration Point", "Job History", "Search"});
			SetAdminPermissions(groupPermissions, new List<string> {"Allow Import", "Allow Export"});
			SetTabVisibility(groupPermissions, new List<string> {"Documents", "Integration Points"});
			SetBrowserPermissions(groupPermissions, new List<string> {"Folders", "Advanced & Saved Searches"});

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

		private static void SetAdminPermissions(GroupPermissions groupPermissions, List<string> permissionNames)
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
