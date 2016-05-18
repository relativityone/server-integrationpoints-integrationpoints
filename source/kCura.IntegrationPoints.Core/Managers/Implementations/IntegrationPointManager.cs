using System;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Managers.Implementations
{
	public class IntegrationPointManager : IIntegrationPointManager
	{
		private readonly IRepositoryFactory _repositoryFactory;

		internal IntegrationPointManager(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
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
			IPermissionRepository permissionRepository = _repositoryFactory.GetPermissionRepository(workspaceArtifactId);

			var permissionCheck = new PermissionCheckDTO() { Success = false };

			if (!permissionRepository.UserCanImport())
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
				if (!permissionRepository.UserCanEditDocuments())
				{
					permissionCheck.ErrorMessage = Constants.IntegrationPoints.NO_PERMISSION_TO_EDIT_DOCUMENTS;

					return permissionCheck;
				}

				dynamic sourceConfiguration = JsonConvert.DeserializeObject(integrationPointDto.SourceConfiguration);
				if (!permissionRepository.UserCanViewArtifact((int)ArtifactType.Search, (int)sourceConfiguration.SavedSearchArtifactId))
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
