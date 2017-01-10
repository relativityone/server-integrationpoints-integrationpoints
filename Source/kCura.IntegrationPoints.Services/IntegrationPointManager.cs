using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointManager : KeplerServiceBase, IIntegrationPointManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="container"></param>
		internal IntegrationPointManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public IntegrationPointManager(ILog logger) : base(logger)
		{
		}

		public void Dispose()
		{
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request)
		{
			CheckIntegrationPointPermissions(request.WorkspaceArtifactId, ArtifactPermission.Create, nameof(CreateIntegrationPointAsync));
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.CreateIntegrationPoint(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(CreateIntegrationPointAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			CheckIntegrationPointPermissions(request.WorkspaceArtifactId, ArtifactPermission.Edit, nameof(UpdateIntegrationPointAsync));
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.UpdateIntegrationPoint(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(UpdateIntegrationPointAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			CheckIntegrationPointPermissions(workspaceArtifactId, ArtifactPermission.View, nameof(GetIntegrationPointAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.GetIntegrationPoint(integrationPointArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetIntegrationPointAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<object> RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			CheckIntegrationPointPermissions(workspaceArtifactId, ArtifactPermission.Edit, nameof(RunIntegrationPointAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(RunIntegrationPointAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			CheckIntegrationPointPermissions(workspaceArtifactId, ArtifactPermission.View, nameof(GetAllIntegrationPointsAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.GetAllIntegrationPoints()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetAllIntegrationPointsAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId)
		{
			CheckIntegrationPointPermissions(workspaceArtifactId, ArtifactPermission.View, nameof(GetOverwriteFieldsChoicesAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.GetOverwriteFieldChoices()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetOverwriteFieldsChoicesAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointFromProfileAsync(int workspaceArtifactId, int profileArtifactId, string integrationPointName)
		{
			CheckCreateIntegrationPointFromProfilePermissions(workspaceArtifactId);
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.CreateIntegrationPointFromProfile(profileArtifactId, integrationPointName)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(CreateIntegrationPointFromProfileAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId)
		{
			CheckIntegrationPointPermissions(workspaceArtifactId, ArtifactPermission.View, nameof(GetIntegrationPointArtifactTypeIdAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.GetIntegrationPointArtifactTypeId()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetIntegrationPointArtifactTypeIdAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			return await new ProviderManager(Logger).GetSourceProviderArtifactIdAsync(workspaceArtifactId, sourceProviderGuidIdentifier).ConfigureAwait(false);
		}

		public async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			return await new ProviderManager(Logger).GetDestinationProviderArtifactIdAsync(workspaceArtifactId, destinationProviderGuidIdentifier).ConfigureAwait(false);
		}

		private void CheckIntegrationPointPermissions(int workspaceId, ArtifactPermission artifactPermission, string endpointName)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasIntegrationPointAddAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), artifactPermission);

				if (hasWorkspaceAccess && hasIntegrationPointAddAccess)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasWorkspaceAccess)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasIntegrationPointAddAccess)
				{
					missingPermissions.Add($"{ObjectTypes.IntegrationPoint} - {artifactPermission}");
				}
				LogAndThrowInsufficientPermissionException(endpointName, missingPermissions);
			});
		}

		private void CheckCreateIntegrationPointFromProfilePermissions(int workspaceId)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasIntegrationPointAddAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.Create);
				bool hasProfileViewAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPointProfile), ArtifactPermission.View);

				if (hasWorkspaceAccess && hasIntegrationPointAddAccess && hasProfileViewAccess)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasWorkspaceAccess)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasIntegrationPointAddAccess)
				{
					missingPermissions.Add($"{ObjectTypes.IntegrationPoint} - {ArtifactPermission.Create}");
				}
				if (!hasProfileViewAccess)
				{
					missingPermissions.Add($"{ObjectTypes.IntegrationPointProfile} - {ArtifactPermission.View}");
				}
				LogAndThrowInsufficientPermissionException(nameof(CreateIntegrationPointFromProfileAsync), missingPermissions);
			});
		}

		protected override Installer Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());
	}
}