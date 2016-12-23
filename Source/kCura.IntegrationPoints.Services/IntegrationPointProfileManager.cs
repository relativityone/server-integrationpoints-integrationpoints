using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointProfileManager : KeplerServiceBase, IIntegrationPointProfileManager
	{
		private IInstaller _installer;

		internal IntegrationPointProfileManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory) : base(logger, permissionRepositoryFactory)
		{
		}

		public IntegrationPointProfileManager(ILog logger) : base(logger)
		{
		}

		protected override IInstaller Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointProfileAsync(CreateIntegrationPointRequest request)
		{
			return
				await Execute((IIntegrationPointProfileRepository repository) => repository.CreateIntegrationPointProfile(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointProfileAsync(CreateIntegrationPointRequest request)
		{
			return
				await Execute((IIntegrationPointProfileRepository repository) => repository.UpdateIntegrationPointProfile(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> GetIntegrationPointProfileAsync(int workspaceArtifactId, int integrationPointProfileArtifactId)
		{
			return
				await
					Execute((IIntegrationPointProfileRepository repository) => repository.GetIntegrationPointProfile(integrationPointProfileArtifactId), workspaceArtifactId)
						.ConfigureAwait(false);
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointProfilesAsync(int workspaceArtifactId)
		{
			return
				await Execute((IIntegrationPointProfileRepository repository) => repository.GetAllIntegrationPointProfiles(), workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId)
		{
			return await Execute((IIntegrationPointProfileRepository repository) => repository.GetOverwriteFieldChoices(), workspaceArtifactId);
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointProfileFromIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId, string profileName)
		{
			return
				await
					Execute((IIntegrationPointProfileRepository repository) => repository.CreateIntegrationPointProfileFromIntegrationPoint(integrationPointArtifactId, profileName),
						workspaceArtifactId);
		}
	}
}