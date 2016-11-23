using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointManager : KeplerServiceBase, IIntegrationPointManager
	{
		private readonly IIntegrationPointRepository _integrationPointRepository;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="integrationPointRepository"></param>
		internal IntegrationPointManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IIntegrationPointRepository integrationPointRepository)
			: base(logger, permissionRepositoryFactory)
		{
			_integrationPointRepository = integrationPointRepository;
		}

		public IntegrationPointManager(ILog logger) : base(logger)
		{
			_integrationPointRepository = new IntegrationPointRepository(logger);
		}

		public void Dispose()
		{
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request)
		{
			return await Execute(() => _integrationPointRepository.CreateIntegrationPointAsync(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			return await Execute(() => _integrationPointRepository.UpdateIntegrationPointAsync(request), request.WorkspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return await Execute(() => _integrationPointRepository.GetIntegrationPointAsync(workspaceArtifactId, integrationPointArtifactId), workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			await Execute(() => _integrationPointRepository.RunIntegrationPointAsync(workspaceArtifactId, integrationPointArtifactId), workspaceArtifactId);
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			return await Execute(() => _integrationPointRepository.GetAllIntegrationPointsAsync(workspaceArtifactId), workspaceArtifactId).ConfigureAwait(false);
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			return await Execute(() => _integrationPointRepository.GetSourceProviderArtifactIdAsync(workspaceArtifactId, sourceProviderGuidIdentifier), workspaceArtifactId);
		}

		public async Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId)
		{
			return await Execute(() => _integrationPointRepository.GetIntegrationPointArtifactTypeIdAsync(workspaceArtifactId), workspaceArtifactId);
		}
	}
}