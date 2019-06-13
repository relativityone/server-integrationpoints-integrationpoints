﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers;
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
			CheckPermissions(nameof(CreateIntegrationPointAsync), request.WorkspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.Create)});
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
			CheckPermissions(nameof(UpdateIntegrationPointAsync), request.WorkspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.Edit)});
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
			CheckPermissions(nameof(GetIntegrationPointAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View)});
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
			CheckPermissions(nameof(RunIntegrationPointAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.Edit)});
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
			CheckPermissions(nameof(GetAllIntegrationPointsAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View)});
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

		public async Task<IList<IntegrationPointModel>> GetEligibleToPromoteIntegrationPointsAsync(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetEligibleToPromoteIntegrationPointsAsync), workspaceArtifactId,
				new[] { new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View) });
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointRepository = container.Resolve<IIntegrationPointRepository>();
					return await Task.Run(() => integrationPointRepository.GetEligibleToPromoteIntegrationPoints()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetEligibleToPromoteIntegrationPointsAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetOverwriteFieldsChoicesAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View)});
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
			CheckPermissions(nameof(CreateIntegrationPointFromProfileAsync), workspaceArtifactId,
				new[]
				{
					new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.Create),
					new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.View)
				});
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
			CheckPermissions(nameof(GetIntegrationPointArtifactTypeIdAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View)});
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

		protected override Installer Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());
	}
}