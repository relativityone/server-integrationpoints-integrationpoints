using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Exceptions;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
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

		/// <summary>
		///     Since we cannot register any dependencies for Kepler Service we have to create separate constructors for runtime
		///     and for testing
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="container"></param>
		protected KeplerServiceBase(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
		{
			_permissionRepositoryFactory = permissionRepositoryFactory;
			_logger = logger;
			_container = container;
		}

		protected KeplerServiceBase(ILog logger) : this(logger, new PermissionRepositoryFactory(), null)
		{
		}

		private readonly ILog _logger;

		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		protected IPermissionRepository GetPermissionRepository(int workspaceId)
		{
			return _permissionRepositoryFactory.Create(global::Relativity.API.Services.Helper, workspaceId);
		}

		protected void CheckPermissions(int workspaceId)
		{
			try
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				if (permissionRepository.UserHasPermissionToAccessWorkspace() &&
					permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View))
				{
					return;
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, _PERMISSIONS_ERROR);
			}
			throw new InsufficientPermissionException(_NO_ACCESS_EXCEPTION_MESSAGE);
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
				_logger.LogError(e, _PERMISSIONS_ERROR);
				throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
			}
		}

		protected void LogAndThrowInsufficientPermissionException(string endpointName, IList<string> missingPermissions)
		{
			_logger.LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", endpointName,
				string.Join(", ", missingPermissions));
			throw new InsufficientPermissionException(_NO_ACCESS_EXCEPTION_MESSAGE);
		}

		protected void LogException(string endpointName, Exception e)
		{
			_logger.LogError(e, "Error occurred during request processing in {endpointName}.", endpointName);
		}

		protected InternalServerErrorException CreateInternalServerErrorException()
		{
			return new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST);
		}

		protected Task<TResult> Execute<TResult, TParameter>(Func<TParameter, TResult> funcToExecute, int workspaceId)
		{
			CheckPermissions(workspaceId);
			try
			{
				using (var container = GetDependenciesContainer(workspaceId))
				{
					TParameter parameter = container.Resolve<TParameter>();
					return Task.Run(() =>
					{
						try
						{
							return funcToExecute(parameter);
						}
						catch (Exception e)
						{
							_logger.LogError(e, "{}", typeof(TParameter));
							throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
						}
					});
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "{}", typeof(TParameter));
				throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
			}
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