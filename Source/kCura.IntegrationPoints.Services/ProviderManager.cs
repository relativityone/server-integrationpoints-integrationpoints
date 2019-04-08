using Castle.Windsor;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Converters;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Repositories;
using LanguageExt;
using Relativity.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

        public async Task<InstallProviderResponse> InstallProviderAsync(InstallProviderRequest request)
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
                    IRipProviderInstaller providerInstaller = container.Resolve<IRipProviderInstaller>();
                    Either<string, Unit> result = await providerInstaller
                        .InstallProvidersAsync(request.ProvidersToInstall.Select(x => x.ToSourceProvider()))
                        .ConfigureAwait(false);

                    return result.Match(
                        success => new InstallProviderResponse(),
                        errorMessage => new InstallProviderResponse(errorMessage),
                        Bottom: () => new InstallProviderResponse(GetAndLogErrorMessageForBottomEitherState())
                    );
                }
            }
            catch (Exception e)
            {
                LogException(nameof(InstallProviderAsync), e);
                throw CreateInternalServerErrorException();
            }
        }

        private string GetAndLogErrorMessageForBottomEitherState([CallerMemberName] string callerName = "")
        {
            Logger.LogFatal("Unexpected state of Either");
            return $"Unexpected error occured in {callerName}";
        }

        public async Task<UninstallProviderResponse> UninstallProviderAsync(UninstallProviderRequest request)
        {
            PermissionModel[] requiredPermissions =
            {
                new PermissionModel(ObjectTypeGuids.SourceProvider, ObjectTypes.SourceProvider, ArtifactPermission.Delete),
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
                    IRipProviderUninstaller providerUninstaller = container.Resolve<IRipProviderUninstaller>();
                    Either<string, Unit> result = await providerUninstaller
                        .UninstallProvidersAsync(request.ApplicationID)
                        .ConfigureAwait(false);

                    return result.Match(
                        success => new UninstallProviderResponse(),
                        errorMessage => new UninstallProviderResponse(errorMessage),
                        Bottom: () => new UninstallProviderResponse(GetAndLogErrorMessageForBottomEitherState())
                    );
                }
            }
            catch (Exception e)
            {
                LogException(nameof(UninstallProviderAsync), e);
                throw CreateInternalServerErrorException();
            }
        }
    }
}