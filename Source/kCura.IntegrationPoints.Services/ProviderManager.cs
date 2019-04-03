﻿using Castle.Windsor;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SourceProvider = kCura.IntegrationPoints.Contracts.SourceProvider;

namespace kCura.IntegrationPoints.Services
{
	public class ProviderManager : KeplerServiceBase, IProviderManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="container"></param>
		internal ProviderManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public ProviderManager(ILog logger) : base(logger)
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new ProviderManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			CheckPermissions(nameof(GetSourceProviderArtifactIdAsync), workspaceArtifactId,
				new[] { new PermissionModel(ObjectTypeGuids.SourceProvider, ObjectTypes.SourceProvider, ArtifactPermission.View) });
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetSourceProviderArtifactId(workspaceArtifactId, sourceProviderGuidIdentifier)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetSourceProviderArtifactIdAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			CheckPermissions(nameof(GetDestinationProviderArtifactIdAsync), workspaceArtifactId,
				new[] { new PermissionModel(ObjectTypeGuids.DestinationProvider, ObjectTypes.DestinationProvider, ArtifactPermission.View) });
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetDestinationProviderArtifactId(workspaceArtifactId, destinationProviderGuidIdentifier)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetDestinationProviderArtifactIdAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<ProviderModel>> GetSourceProviders(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetSourceProviders), workspaceArtifactId,
				new[] { new PermissionModel(ObjectTypeGuids.SourceProvider, ObjectTypes.SourceProvider, ArtifactPermission.View) });
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetSourceProviders(workspaceArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetSourceProviders), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<ProviderModel>> GetDestinationProviders(int workspaceArtifactId)
		{
			CheckPermissions(nameof(GetDestinationProviders), workspaceArtifactId,
				new[] { new PermissionModel(ObjectTypeGuids.DestinationProvider, ObjectTypes.DestinationProvider, ArtifactPermission.View) });
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetDesinationProviders(workspaceArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetDestinationProviders), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<bool> InstallProviderAsync(InstallProviderRequest request)
		{
			PermissionModel[] requiredPermissions =
			{
				new PermissionModel(ObjectTypeGuids.SourceProvider, ObjectTypes.SourceProvider, ArtifactPermission.Create),
				new PermissionModel(ObjectTypeGuids.SourceProvider, ObjectTypes.SourceProvider, ArtifactPermission.Edit)
			};

			CheckPermissions(
				nameof(GetDestinationProviders),
				request.WorkspaceID,
				requiredPermissions);
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(request.WorkspaceID))
				{
					ProviderInstaller providerInstaller = container.Resolve<ProviderInstaller>();
					await providerInstaller.InstallProvidersAsync(request.ProvidersToInstall.Select(ConvertToSourceProvider));
					return true;
				}
			}
			catch (Exception e)
			{
				LogException(nameof(InstallProviderAsync), e);
				throw CreateInternalServerErrorException(); // TODO verify it
			}
		}

		public async Task<bool> UninstallProviderAsync(UninstallProviderRequest request)
		{
			PermissionModel[] requiredPermissions =
			{
				new PermissionModel(ObjectTypeGuids.DestinationProvider, ObjectTypes.DestinationProvider, ArtifactPermission.Delete),
				new PermissionModel(ObjectTypeGuids.IntegrationPoint, ObjectTypes.IntegrationPoint, ArtifactPermission.Edit),
				new PermissionModel(ObjectTypeGuids.IntegrationPoint, ObjectTypes.IntegrationPoint, ArtifactPermission.Delete)
			};

			CheckPermissions(
				nameof(GetDestinationProviders),
				request.WorkspaceID,
				requiredPermissions);
			try
			{
				using (IWindsorContainer container = GetDependenciesContainer(request.WorkspaceID))
				{
					ProviderUninstaller providerUninstaller = container.Resolve<ProviderUninstaller>();
					await providerUninstaller.UninstallProvidersAsync(request.ApplicationID);
					return true;
				}
			}
			catch (Exception e)
			{
				LogException(nameof(UninstallProviderAsync), e);
				throw CreateInternalServerErrorException(); // TODO verify it
			}
		}

		private SourceProvider ConvertToSourceProvider(ProviderToInstallDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new SourceProvider
			{
				Name = dto.Name,
				Url = dto.Url,
				ViewDataUrl = dto.ViewDataUrl,
				ApplicationID = dto.ApplicationID,
				ApplicationGUID = dto.ApplicationGUID,
				GUID = dto.GUID,
				Configuration = ConvertToSourceProviderConfiguration(dto.Configuration)
			};
		}

		private SourceProviderConfiguration ConvertToSourceProviderConfiguration(ProviderToInstallConfigurationDto dto)
		{
			if (dto == null)
			{
				return null;
			}

			return new SourceProviderConfiguration
			{
				AlwaysImportNativeFileNames = dto.AlwaysImportNativeFileNames,
				AlwaysImportNativeFiles = dto.AlwaysImportNativeFiles,
				CompatibleRdoTypes = dto.CompatibleRdoTypes,
				OnlyMapIdentifierToIdentifier = dto.OnlyMapIdentifierToIdentifier,
				AvailableImportSettings = new ImportSettingVisibility
				{
					AllowUserToMapNativeFileField = dto.AllowUserToMapNativeFileField
				}
			};
		}
	}
}