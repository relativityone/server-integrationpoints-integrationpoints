using System;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Helpers;
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
		///     Since we cannot register any dependencies for Kepler Service we have to create separate constructors for runtime
		///     and for testing
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		protected KeplerServiceBase(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory)
		{
			_permissionRepositoryFactory = permissionRepositoryFactory;
			Logger = logger;
		}

		protected KeplerServiceBase(ILog logger) : this(logger, new PermissionRepositoryFactory())
		{
		}

		public ILog Logger { get; }

		public async Task<bool> PingAsync()
		{
			return await Task.Run(() => true).ConfigureAwait(false);
		}

		protected void CheckPermissions(int workspaceId)
		{
			try
			{
				var permissionRepository = _permissionRepositoryFactory.Create(global::Relativity.API.Services.Helper, workspaceId);
				if (permissionRepository.UserHasPermissionToAccessWorkspace() &&
					permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.IntegrationPoint), ArtifactPermission.View))
				{
					return;
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e, _PERMISSIONS_ERROR);
			}
			throw new InsufficientPermissionException(_NO_ACCESS_EXCEPTION_MESSAGE);
		}

		protected Task<T> Execute<T>(Func<Task<T>> a, int workspaceId)
		{
			CheckPermissions(workspaceId);
			try
			{
				return Task.Run(a);
			}
			catch (Exception e)
			{
				throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
			}
		}

		protected Task<T> Execute<T>(Func<T> a, int workspaceId)
		{
			CheckPermissions(workspaceId);
			try
			{
				return Task.Run(a);
			}
			catch (Exception e)
			{
				throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
			}
		}

		protected Task Execute(Func<Task> a, int workspaceId)
		{
			CheckPermissions(workspaceId);
			try
			{
				return Task.Run(a);
			}
			catch (Exception e)
			{
				throw new InternalServerErrorException(_ERROR_OCCURRED_DURING_REQUEST, e);
			}
		}
	}
}