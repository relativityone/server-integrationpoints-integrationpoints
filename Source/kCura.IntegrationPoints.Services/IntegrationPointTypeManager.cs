using System.Collections.Generic;
using System.Threading.Tasks;
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
		internal IntegrationPointTypeManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory) : base(logger, permissionRepositoryFactory)
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
			return
				await Execute((IIntegrationPointTypeRepository integrationPointTypeRepository) => integrationPointTypeRepository.GetIntegrationPointTypes(), workspaceArtifactId);
		}
	}
}