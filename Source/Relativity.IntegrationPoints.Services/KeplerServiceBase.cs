using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Installers;
using Relativity.Logging;

namespace Relativity.IntegrationPoints.Services
{
    public abstract class KeplerServiceBase : IKeplerService
    {
        private const string _NO_ACCESS_EXCEPTION_MESSAGE = "You do not have permission to access this service.";
        private const string _ERROR_OCCURRED_DURING_REQUEST = "Error occurred during request processing. Please contact your administrator.";
        private const string _PERMISSIONS_ERROR = "Failed to validate permissions for Integration Points Kepler Service. Denying access.";

        private readonly IPermissionRepositoryFactory _permissionRepositoryFactory;

        /// <summary>
        ///     This container is used only for testing purposes
        /// </summary>
        private readonly IWindsorContainer _container;

        protected readonly ILog Logger;

        /// <summary>
        ///     Since we cannot register any dependencies for Kepler Service we have to create separate constructors for runtime
        ///     and for testing
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="permissionRepositoryFactory"></param>
        /// <param name="container"></param>
        protected KeplerServiceBase(
            ILog logger,
            IPermissionRepositoryFactory permissionRepositoryFactory,
            IWindsorContainer container)
        {
            _permissionRepositoryFactory = permissionRepositoryFactory;
            Logger = logger;
            _container = container;
        }

        protected KeplerServiceBase(ILog logger)
            : this(logger, new PermissionRepositoryFactory(), container: null)
        {
        }

        public async Task<bool> PingAsync()
        {
            return await Task.Run(() => true).ConfigureAwait(false);
        }

        protected void SafePermissionCheck(Action checkPermission)
        {
            try
            {
                checkPermission();
            }
            catch (InsufficientPermissionException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, _PERMISSIONS_ERROR);
                throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
            }
        }

        protected void CheckPermissions(string endpointName, int workspaceId)
        {
            CheckPermissions(endpointName, workspaceId, new List<PermissionModel>());
        }

        protected void CheckPermissions(string endpointName, int workspaceId, IEnumerable<PermissionModel> permissionsToCheck)
        {
            SafePermissionCheck(() =>
            {
#pragma warning disable CS0618 // Type or member is obsolete REL-292860
                IPermissionRepository permissionRepository = _permissionRepositoryFactory.Create(global::Relativity.API.Services.Helper, workspaceId);
#pragma warning restore CS0618 // Type or member is obsolete
                var missingPermissions = new List<string>();
                if (!permissionRepository.UserHasPermissionToAccessWorkspace())
                {
                    missingPermissions.Add("Workspace");
                }
                foreach (PermissionModel permissionModel in permissionsToCheck)
                {
                    if (!permissionRepository.UserHasArtifactTypePermission(permissionModel.ObjectTypeGuid, permissionModel.ArtifactPermission))
                    {
                        missingPermissions.Add($"{permissionModel.ObjectTypeName} - {permissionModel.ArtifactPermission}");
                    }
                }
                if (missingPermissions.Count == 0)
                {
                    return;
                }
                LogAndThrowInsufficientPermissionException(endpointName, missingPermissions);
            });
        }

        protected void LogAndThrowInsufficientPermissionException(string endpointName, IList<string> missingPermissions)
        {
            Logger.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", endpointName,
                string.Join(", ", missingPermissions));
            throw new InsufficientPermissionException(_NO_ACCESS_EXCEPTION_MESSAGE);
        }

        protected void LogInvocation(string endpointName)
        {
            Logger.LogInformation("Integration Points private service endpoint invoked: {endpointName}", endpointName);
        }

        protected void LogException(string endpointName, Exception e)
        {
            Logger.LogError(e, "Error occurred during request processing in {endpointName}.", endpointName);
        }

        protected InternalServerErrorException CreateInternalServerErrorException()
        {
            return new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST);
        }

        protected virtual IWindsorContainer GetDependenciesContainer(int workspaceArtifactId)
        {
            if (_container != null)
            {
                return _container;
            }
            IWindsorContainer container = new WindsorContainer();
            Installer.Install(container, new DefaultConfigurationStore(), workspaceArtifactId);
            return container;
        }

        protected abstract Installer Installer { get; }
    }
}