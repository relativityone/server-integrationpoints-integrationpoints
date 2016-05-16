using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class PermissionRepository : IPermissionRepository
	{
		private readonly IServicesMgr _servicesMgr;
		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		public PermissionRepository(IServicesMgr servicesMgr)
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

		public bool UserCanViewArtifact(int workspaceId, int artifactTypeId, int artifactId)
		{
			var permission = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier(artifactTypeId),
				PermissionType = PermissionType.View
			};

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _servicesMgr.CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(workspaceId,
						new List<PermissionRef>() {permission}, artifactId);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.First().Selected;
					}
				}
				catch 
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
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