using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class DestinationProviderRepository : IDestinationProviderRepository
	{
		private readonly IRelativityObjectManager _relativityObjectManager;
		private readonly IDestinationProviderArtifactIdByGuidQueryBuilder _artifactIdByGuid = new DestinationProviderArtifactIdByGuidQueryBuilder();

		public DestinationProviderRepository(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		public int GetArtifactIdFromDestinationProviderTypeGuidIdentifier(string destinationProviderGuidIdentifier)
		{
			QueryRequest query = _artifactIdByGuid.Create(destinationProviderGuidIdentifier);

			List<RelativityObject> queryResults;
			try
			{
				queryResults = _relativityObjectManager.Query(query);
				return queryResults.Single().ArtifactID;
			}
			catch (Exception e)
			{
				throw new IntegrationPointsException($"Failed to retrieve Destination Provider Artifact Id for guid: {destinationProviderGuidIdentifier}.", e);
			}
		}
	}

}