using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Newtonsoft.Json;

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

		public Constants.SourceProvider GetSourceProvider(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			ISourceProviderRepository repository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO dto = repository.Read(integrationPointDto.SourceProvider.Value);

			Constants.SourceProvider sourceProvider;
			if (dto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID))
			{
				sourceProvider = Constants.SourceProvider.Relativity;
			}
			else
			{
				sourceProvider = Constants.SourceProvider.Other;
			}

			return sourceProvider;
		}

		public PermissionCheckDTO UserHasPermissions(int workspaceArtifactId, IntegrationPointDTO integrationPointDto, Constants.SourceProvider? sourceProvider = null)
		{
			var permissionCheck = new PermissionCheckDTO() { Success = false };

			if (!_permissionRepository.UserCanImport(workspaceArtifactId))
			{
				permissionCheck.ErrorMessage = Constants.IntegrationPoints.NO_PERMISSION_TO_IMPORT_CURRENTWORKSPACE;

				return permissionCheck;
			}

			if (!sourceProvider.HasValue)
			{
				sourceProvider = this.GetSourceProvider(workspaceArtifactId, integrationPointDto);
			}

			if (sourceProvider == Constants.SourceProvider.Relativity)
			{
				if (!_permissionRepository.UserCanEditDocuments(workspaceArtifactId))
				{
					permissionCheck.ErrorMessage = Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS;

					return permissionCheck;
				}

				dynamic sourceConfiguration = JsonConvert.DeserializeObject(integrationPointDto.SourceConfiguration);
				if (!_permissionRepository.UserCanViewArtifact(workspaceArtifactId, (int)ArtifactType.Search,
					(int)sourceConfiguration.SavedSearchArtifactId))
				{
					permissionCheck.ErrorMessage = Constants.IntegrationPoints.NO_PERMISSION_TO_ACCESS_SAVEDSEARCH;

					return permissionCheck;
				}
			}

			permissionCheck.Success = true;

			return permissionCheck;
		}
	}
}
