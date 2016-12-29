using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Newtonsoft.Json;
using Relativity;

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
			ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			SourceProviderDTO sourceProviderDto = sourceProviderRepository.Read(integrationPointDto.SourceProvider.Value);
			
			var sourceProvider = Constants.SourceProvider.Other;

			var destinationProvider = GetDestinationProvider(workspaceArtifactId, integrationPointDto);

			if ((sourceProviderDto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)) &&
				(destinationProvider == Constants.DestinationProvider.Relativity))
			{
				sourceProvider = Constants.SourceProvider.Relativity;
			}

			return sourceProvider;
		}

		private Constants.DestinationProvider GetDestinationProvider(int workspaceArtifactId, IntegrationPointDTO integrationPointDto)
		{
			IDestinationProviderRepository destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
			DestinationProviderDTO destinationProviderDto = destinationProviderRepository.Read(integrationPointDto.DestinationProvider.Value);

			if (destinationProviderDto.Identifier == new Guid(Constants.IntegrationPoints.RELATIVITY_DESTINATION_PROVIDER_GUID))
			{
				return Constants.DestinationProvider.Relativity;
			}
			if (destinationProviderDto.Identifier == new Guid(Constants.IntegrationPoints.LOAD_FILE_DESTINATION_PROVIDER_GUID))
			{
				return Constants.DestinationProvider.LoadFile;
			}
			return Constants.DestinationProvider.Other;
		}
	}
}