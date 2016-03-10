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
		private const int ALLOW_IMPORT_PERMISSION_ID = 158;

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
					PermissionID = ALLOW_IMPORT_PERMISSION_ID
				};

				Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(workspaceId, new List<PermissionRef>() {allowImportPermission});
				List<PermissionValue> permissionValues = permissionValuesTask.Result;

				if (permissionValues == null || !permissionValues.Any())
				{
					return false;
				}

				PermissionValue allowImportPermissionValue = permissionValues.First();
				bool userHasImportPermissions = allowImportPermissionValue.Selected &&
				                                allowImportPermissionValue.PermissionID == ALLOW_IMPORT_PERMISSION_ID;

				return userHasImportPermissions;
			}
		}

	}
}
