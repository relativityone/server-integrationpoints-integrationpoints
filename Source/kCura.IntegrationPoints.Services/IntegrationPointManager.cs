using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointManager : KeplerServiceBase, IIntegrationPointManager
	{
		private IInstaller _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		internal IntegrationPointManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory) : base(logger, permissionRepositoryFactory)
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
			return
				await
					Execute((IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.CreateIntegrationPoint(request), request.WorkspaceArtifactId)
						.ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			return
				await
					Execute((IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.UpdateIntegrationPoint(request), request.WorkspaceArtifactId)
						.ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return
				await
					Execute(
						(IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.GetIntegrationPoint(integrationPointArtifactId),
						workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<object> RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return
				await
					Execute(
						(IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId),
						workspaceArtifactId);
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			return
				await
					Execute((IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.GetAllIntegrationPoints(), workspaceArtifactId)
						.ConfigureAwait(false);
		}

		public async Task<IList<OverwriteFieldsModel>> GetOverwriteFieldsChoicesAsync(int workspaceArtifactId)
		{
			return await Execute((IChoiceRepository choiceRepository) => choiceRepository.GetOverwriteFieldChoices(), workspaceArtifactId);
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			return
				await
					Execute(
						(IProviderRepository providerRepository) =>
								providerRepository.GetSourceProviderArtifactId(workspaceArtifactId, sourceProviderGuidIdentifier), workspaceArtifactId);
		}

		public async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			return
				await
					Execute(
						(IProviderRepository providerRepository) =>
								providerRepository.GetDestinationProviderArtifactId(workspaceArtifactId, destinationProviderGuidIdentifier), workspaceArtifactId);
		}

		public async Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId)
		{
			return
				await
					Execute((IIntegrationPointRepository integrationPointRepository) => integrationPointRepository.GetIntegrationPointArtifactTypeId(),
						workspaceArtifactId);
		}

		protected override IInstaller Installer => _installer ?? (_installer = new IntegrationPointManagerInstaller());
	}
}