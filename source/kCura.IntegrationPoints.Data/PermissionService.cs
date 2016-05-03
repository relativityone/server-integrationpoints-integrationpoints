using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data
{
	public class PermissionService : IPermissionService
	{
		private readonly IServicesMgr _servicesMgr;
		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		public PermissionService(IServicesMgr servicesMgr)
		{
			_servicesMgr = servicesMgr;
		}

		public bool UserCanImport(int workspaceId)
		{
			return HasPermissions(workspaceId, _ALLOW_IMPORT_PERMISSION_ID);
		}

		public bool UserCanEditDocuments(int workspaceId)
		{
			return HasPermissions(workspaceId, _EDIT_DOCUMENT_PERMISSION_ID);
		}

		internal bool HasPermissions(int workspaceId, int permissionToCheck)
		{
			using (IPermissionManager proxy = _servicesMgr.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				var permission = new PermissionRef()
				{
					PermissionID = permissionToCheck
				};

				bool hasPermission = false;
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(workspaceId,
						new List<PermissionRef>() { permission });
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues == null || !permissionValues.Any())
					{
						return false;
					}

					PermissionValue hasPermissionValue = permissionValues.First();
					hasPermission = hasPermissionValue.Selected &&
												hasPermissionValue.PermissionID == permissionToCheck;
				}
				catch
				{
					// invalid IDs will cause the request to except
					// suppress these errors and do not give the user access	
				}

				return hasPermission;
			}
		}

	}
}
