using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointTypeManager : KeplerServiceBase, IIntegrationPointTypeManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="container"></param>
		internal IntegrationPointTypeManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public IntegrationPointTypeManager(ILog logger) : base(logger)
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new IntegrationPointTypeManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<IList<IntegrationPointTypeModel>> GetIntegrationPointTypes(int workspaceArtifactId)
		{
			CheckIntegrationPointTypePermissions(workspaceArtifactId);
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointTypeRepository = container.Resolve<IIntegrationPointTypeRepository>();
					return await Task.Run(() => integrationPointTypeRepository.GetIntegrationPointTypes()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetIntegrationPointTypes), e);
				throw CreateInternalServerErrorException();
			}
		}

		private void CheckIntegrationPointTypePermissions(int workspaceId)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasIntegrationPointTypeAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointType), ArtifactPermission.View);
				if (hasWorkspaceAccess && hasIntegrationPointTypeAccess)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasWorkspaceAccess)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasIntegrationPointTypeAccess)
				{
					missingPermissions.Add($"{ObjectTypes.IntegrationPointType} - View");
				}
				LogAndThrowInsufficientPermissionException(nameof(GetIntegrationPointTypes), missingPermissions);
			});
		}
	}
}