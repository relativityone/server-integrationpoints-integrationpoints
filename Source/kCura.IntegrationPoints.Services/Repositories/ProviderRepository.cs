using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class ProviderRepository : IProviderRepository
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRSAPIService _rsapiService;

		public ProviderRepository(IRepositoryFactory repositoryFactory, IRSAPIService rsapiService)
		{
			_repositoryFactory = repositoryFactory;
			_rsapiService = rsapiService;
		}

		public int GetSourceProviderArtifactId(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			ISourceProviderRepository sourceProviderRepository = _repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
			return sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(sourceProviderGuidIdentifier);
		}

		public IList<ProviderModel> GetSourceProviders(int workspaceArtifactId)
		{
			var sourceProviders = _rsapiService.SourceProviderLibrary.Query(new AllSourceProvidersWithNameQueryBuilder().Create());
			return sourceProviders.Select(x => x.ToModel()).ToList();
		}

		public IList<ProviderModel> GetDesinationProviders(int workspaceArtifactId)
		{
			var destinationProviders = _rsapiService.DestinationProviderLibrary.Query(new AllDestinationProvidersWithNameQueryBuilder().Create());
			return destinationProviders.Select(x => x.ToModel()).ToList();
		}

		public int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			IDestinationProviderRepository destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
			return destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(destinationProviderGuidIdentifier);
		}
	}
}