using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Permission;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class PermissionRepository : IPermissionRepository
	{
		private readonly IHelper _helper;
		private readonly int _workspaceArtifactId;
		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
		private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission

		public PermissionRepository(IHelper helper, int workspaceArtifactId)
		{
			_helper = helper;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public bool UserCanImport()
		{
			return HasPermissions(_workspaceArtifactId, _ALLOW_IMPORT_PERMISSION_ID);
		}

		public bool UserCanExport()
		{
			return HasPermissions(_workspaceArtifactId, _ALLOW_EXPORT_PERMISSION_ID);
		}

		public bool UserHasArtifactInstancePermissions(int artifactTypeId, int artifactId, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			throw new NotImplementedException();
		}

		public bool UserHasPermissionToAccessWorkspace()
		{
			var permissionRef = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier((int) ArtifactType.Case),
				PermissionType = PermissionType.View
			};

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(-1, new List<PermissionRef>() { permissionRef }, _workspaceArtifactId);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.All(x => x.Selected);
					}
				}
				catch
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
		}

		public bool UserCanEditDocuments()
		{
			return HasPermissions(_workspaceArtifactId, _EDIT_DOCUMENT_PERMISSION_ID);
		}

		public bool UserHasArtifactInstancePermission(Guid artifactTypeGuid, int artifactId, ArtifactPermission artifactPermission)
		{
			return UserHasArtifactTypePermissions(artifactTypeGuid, new[] { artifactPermission });
		}

		public bool UserHasArtifactInstancePermissions(Guid artifactTypeGuid, int artifactId, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			List<PermissionRef> permissionRefs = artifactPermissions.Select(x => new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier(artifactTypeGuid),
				PermissionType = this.ArtifactPermissionToPermissinType(x)
			}).ToList();

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(_workspaceArtifactId, permissionRefs);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.All(x => x.Selected);
					}
				}
				catch
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
		}

		private bool UserHasArtifactTypePermissions(ArtifactTypeIdentifier artifactTypeIdentifier, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			List<PermissionRef> permissionRefs = artifactPermissions.Select(x => new PermissionRef()
			{
				ArtifactType = artifactTypeIdentifier,
				PermissionType = this.ArtifactPermissionToPermissinType(x)
			}).ToList();

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(_workspaceArtifactId, permissionRefs);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.All(x => x.Selected);
					}
				}
				catch
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
		}

		private bool UserHasArtifactInstancePermissions(ArtifactTypeIdentifier artifactTypeIdentifier, int artifactId, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			List<PermissionRef> permissionRefs = artifactPermissions.Select(x => new PermissionRef()
			{
				ArtifactType = artifactTypeIdentifier,
				PermissionType = this.ArtifactPermissionToPermissinType(x)
			}).ToList();

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(_workspaceArtifactId, permissionRefs, artifactId);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.All(x => x.Selected);
					}
				}
				catch
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
		}

		public bool UserHasArtifactTypePermissions(int artifactTypeId, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			return this.UserHasArtifactTypePermissions(new ArtifactTypeIdentifier(artifactTypeId), artifactPermissions);
		}

		public bool UserHasArtifactTypePermission(int artifactTypeId, ArtifactPermission artifactPermission)
		{
			return this.UserHasArtifactTypePermissions(new ArtifactTypeIdentifier(artifactTypeId), new[] {artifactPermission});
		}

		public bool UserHasArtifactInstancePermission(int artifactTypeId, int artifactId, ArtifactPermission artifactPermission)
		{
			return this.UserHasArtifactInstancePermissions(new ArtifactTypeIdentifier(artifactTypeId), artifactId, new[] {artifactPermission});
		}

		public bool UserHasArtifactTypePermission(Guid artifactTypeGuid, ArtifactPermission artifactPermission)
		{
			return UserHasArtifactTypePermissions(artifactTypeGuid, new[] { artifactPermission });
		}

		public bool UserHasArtifactTypePermissions(Guid artifactTypeGuid, IEnumerable<ArtifactPermission> artifactPermissions)
		{
			List<PermissionRef> permissionRefs = artifactPermissions.Select(x => new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier(artifactTypeGuid),
				PermissionType = this.ArtifactPermissionToPermissinType(x)
			}).ToList();

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(_workspaceArtifactId, permissionRefs);
					List<PermissionValue> permissionValues = permissionValuesTask.Result;

					if (permissionValues != null && permissionValues.Any())
					{
						userHasViewPermissions = permissionValues.All(x => x.Selected);
					}
				}
				catch
				{
					// If the user does not have permissions, the kepler service throws an exception
				}
			}

			return userHasViewPermissions;
		}

		private PermissionType ArtifactPermissionToPermissinType(ArtifactPermission artifactPermission)
		{
			switch (artifactPermission)
			{
				case ArtifactPermission.View:
					return PermissionType.View;
				case ArtifactPermission.Edit:
					return PermissionType.Edit;
				case ArtifactPermission.Create:
					return PermissionType.Add;
				default:
					throw new System.Exception("Invalid ArtifactPermission");
			}
		}

		public bool UserCanViewArtifact(int artifactTypeId, int artifactId)
		{
			var permission = new PermissionRef()
			{
				ArtifactType = new ArtifactTypeIdentifier(artifactTypeId),
				PermissionType = PermissionType.View
			};

			bool userHasViewPermissions = false;

			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
			{
				try
				{
					Task<List<PermissionValue>> permissionValuesTask = proxy.GetPermissionSelectedAsync(_workspaceArtifactId,
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
			using (IPermissionManager proxy = _helper.GetServicesManager().CreateProxy<IPermissionManager>(ExecutionIdentity.CurrentUser))
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