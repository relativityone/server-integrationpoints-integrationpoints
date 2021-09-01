using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
{
	public class IntegrationPointProfileManager : KeplerServiceBase, IIntegrationPointProfileManager
	{
		private Installer _installer;

		internal IntegrationPointProfileManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}
		
		public IntegrationPointProfileManager(ILog logger) : base(logger)
		{
		}

        protected override Installer Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointProfileAsync(CreateIntegrationPointRequest request)
		{
			LogInvocation(nameof(CreateIntegrationPointProfileAsync));

			CheckPermissions(nameof(CreateIntegrationPointProfileAsync), request.WorkspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.Create)});
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return await Task.Run(() => integrationPointProfileRepository.CreateIntegrationPointProfile(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(CreateIntegrationPointProfileAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointProfileAsync(CreateIntegrationPointRequest request)
		{
			LogInvocation(nameof(UpdateIntegrationPointProfileAsync));

			CheckPermissions(nameof(UpdateIntegrationPointProfileAsync), request.WorkspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.Edit)});
			try
			{
				using (var container = GetDependenciesContainer(request.WorkspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return await Task.Run(() => integrationPointProfileRepository.UpdateIntegrationPointProfile(request)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(UpdateIntegrationPointProfileAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> GetIntegrationPointProfileAsync(int workspaceArtifactId, int integrationPointProfileArtifactId)
		{
			CheckPermissions(nameof(GetIntegrationPointProfileAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.View)});
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return await Task.Run(() => integrationPointProfileRepository.GetIntegrationPointProfile(integrationPointProfileArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetIntegrationPointProfileAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointProfilesAsync(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetAllIntegrationPointProfilesAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.View)});
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return await Task.Run(() => integrationPointProfileRepository.GetAllIntegrationPointProfiles()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetAllIntegrationPointProfilesAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetOverwriteFieldsChoicesAsync), workspaceArtifactId,
				new[] {new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.View)});
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return await Task.Run(() => integrationPointProfileRepository.GetOverwriteFieldChoices()).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetOverwriteFieldsChoicesAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointProfileFromIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId,
			string profileName)
		{
			LogInvocation(nameof(CreateIntegrationPointProfileFromIntegrationPointAsync));

			CheckPermissions(nameof(CreateIntegrationPointProfileFromIntegrationPointAsync), workspaceArtifactId,
				new[]
				{
					new PermissionModel(ObjectTypeGuids.IntegrationPointProfileGuid, ObjectTypes.IntegrationPointProfile, ArtifactPermission.Create),
					new PermissionModel(ObjectTypeGuids.IntegrationPointGuid, ObjectTypes.IntegrationPoint, ArtifactPermission.View)
				});
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var integrationPointProfileRepository = container.Resolve<IIntegrationPointProfileRepository>();
					return
						await
							Task.Run(() => integrationPointProfileRepository.CreateIntegrationPointProfileFromIntegrationPoint(integrationPointArtifactId, profileName)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(CreateIntegrationPointProfileFromIntegrationPointAsync), e);
				throw CreateInternalServerErrorException();
			}
		}
	}
}