﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
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
			var sourceProviders = _rsapiService.RelativityObjectManager.Query<SourceProvider>(new QueryRequest()
			{
				Fields = new List<FieldRef>() {new FieldRef() { Guid = SourceProviderFieldGuids.NameGuid },
					new FieldRef() { Guid = SourceProviderFieldGuids.IdentifierGuid }
				}
			});
			return sourceProviders.Select(Mapper.Map<ProviderModel>).ToList();
		}

		public IList<ProviderModel> GetDesinationProviders(int workspaceArtifactId)
		{
			var destinationProviders = _rsapiService.RelativityObjectManager.Query<DestinationProvider>(new QueryRequest()
			{
				Fields = new List<FieldRef>() {
					new FieldRef() { Guid = DestinationProviderFieldGuids.NameGuid },
					new FieldRef() { Guid = DestinationProviderFieldGuids.IdentifierGuid}
				}
			});
			return destinationProviders.Select(Mapper.Map<ProviderModel>).ToList();
		}

		public int GetDestinationProviderArtifactId(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			IDestinationProviderRepository destinationProviderRepository = _repositoryFactory.GetDestinationProviderRepository(workspaceArtifactId);
			return destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(destinationProviderGuidIdentifier);
		}
	}
}