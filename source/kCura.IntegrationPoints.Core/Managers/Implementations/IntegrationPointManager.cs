using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class IntegrationPointManager : IIntegrationPointManager
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IPermissionRepository _permissionRepository;

		internal IntegrationPointManager(IRepositoryFactory repositoryFactory, IPermissionRepository permissionRepository)
		{
			_repositoryFactory = repositoryFactory;
			_permissionRepository = permissionRepository;
		}

		public IntegrationPointDTO Read(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IIntegrationPointRepository repository = _repositoryFactory.GetIntegrationPointRepository(workspaceArtifactId);

			return repository.Read(integrationPointArtifactId);
		}

		public bool IntegrationPointTypeIsRetriable(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			ISourceProviderRepository repository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO dto = repository.Read(integrationPointDto.SourceProvider.Value);

			bool retriable = dto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);

			return retriable;
		}

		public bool UserHasPermissions(int workspaceArtifactId)
		{
			bool userCanEditDocuments = _permissionRepository.UserCanEditDocuments(workspaceArtifactId);
			bool userCanImport = _permissionRepository.UserCanImport(workspaceArtifactId);
			bool userHasPermissions = userCanEditDocuments && userCanImport;
			return userHasPermissions;
		}
	}
}
