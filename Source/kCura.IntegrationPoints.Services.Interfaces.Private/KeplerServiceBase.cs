using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public abstract class KeplerServiceBase : IKeplerService
	{
		private const string _NO_ACCESS_EXCEPTION_MESSAGE = "You do not have permission to access this service.";

		private readonly IPermissionRepositoryFactory _permissionRepositoryFactory;

		/// <summary>
		///     Since we cannot register any dependencies for Kepler Service we have to create separate constructors for runtime
		///     and for testing
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositryFactory"></param>
		protected KeplerServiceBase(ILog logger, IPermissionRepositoryFactory permissionRepositryFactory)
		{
			_permissionRepositoryFactory = permissionRepositryFactory;
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
				Logger.LogError(e, "Failed to validate permissions for Integration Points Kepler Service. Denying access.");
			}
			throw new Exception(_NO_ACCESS_EXCEPTION_MESSAGE);
		}
	}
}