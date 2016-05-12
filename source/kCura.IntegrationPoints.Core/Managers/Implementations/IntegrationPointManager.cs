using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;

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

		public bool IntegrationPointSourceProviderIsRelativity(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			ISourceProviderRepository repository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO dto = repository.Read(integrationPointDto.SourceProvider.Value);

			bool isRelativityProvider = dto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID);

			return isRelativityProvider;
		}

		public bool UserHasPermissions(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			bool userCanEditDocuments = _permissionRepository.UserCanEditDocuments(workspaceArtifactId);
			bool userCanImport = _permissionRepository.UserCanImport(workspaceArtifactId);
			bool userCanAccessSavedSearch = _permissionRepository.UserCanViewArtifact(workspaceArtifactId, (int) ArtifactType.Search, integrationPointDto.SourceProvider.Value);
			bool userHasPermissions = userCanEditDocuments && userCanImport && userCanAccessSavedSearch;

			return userHasPermissions;
		}
	}
}
