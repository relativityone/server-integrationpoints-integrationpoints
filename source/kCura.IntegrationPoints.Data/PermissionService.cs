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
			using (IPermissionManager proxy = _servicesMgr.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				var allowImportPermission = new PermissionRef()
				{
					PermissionID = _ALLOW_IMPORT_PERMISSION_ID
				};

				bool userHasImportPermissions = false;
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(workspaceId,
						new List<PermissionRef>() {allowImportPermission});
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues == null || !permissionValues.Any())
					{
						return false;
					}

					PermissionValue allowImportPermissionValue = permissionValues.First();
					userHasImportPermissions = allowImportPermissionValue.Selected &&
												allowImportPermissionValue.PermissionID == _ALLOW_IMPORT_PERMISSION_ID;
				}
				catch 
				{
					// invalid IDs will cause the request to except
					// suppress these errors and do not give the user access	
				}

				return userHasImportPermissions;
			}
		}

		public bool UserCanEditDocuments(int workspaceId)
		{
			using (IPermissionManager proxy = _servicesMgr.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				var editDocumentPermission = new PermissionRef()
				{
					PermissionID = _EDIT_DOCUMENT_PERMISSION_ID
				};

				bool userHasEditDocumentPermissions = false;
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(workspaceId,
						new List<PermissionRef>() { editDocumentPermission });
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues == null || !permissionValues.Any())
					{
						return false;
					}

					PermissionValue editDocumentPermissionValue = permissionValues.First();
					userHasEditDocumentPermissions = editDocumentPermissionValue.Selected &&
												editDocumentPermissionValue.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID;
				}
				catch
				{
					// invalid IDs will cause the request to except
					// suppress these errors and do not give the user access	
				}

				return userHasEditDocumentPermissions;
			}
		}

	}
}
