using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
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

		public DestinationProviderDTO Read(int artifactId)
		{
			var nameFieldRef = new FieldRef { Name = SourceProviderFields.Name };
			var identifierFieldRef = new FieldRef { Name = SourceProviderFields.Identifier };
			var queryRequest = new QueryRequest
			{
				Condition = $"'{Domain.Constants.DESTINATION_PROVIDER_ARTIFACTID_FIELD_NAME}' == {artifactId}",
				Fields = new List<FieldRef> { nameFieldRef, identifierFieldRef },
				ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.DestinationProvider) }
			};
			List<DestinationProvider> queryResults;
			try
			{
				queryResults = _relativityObjectManager.Query<DestinationProvider>(queryRequest);
				return CreateDestinationProviderDTO(queryResults.Single());
			}
			catch (Exception e)
			{
				throw new IntegrationPointsException($"Failed to retrieve Destination Provider for artifact Id: {artifactId}", e);
			}
		}

		public int GetArtifactIdFromDestinationProviderTypeGuidIdentifier(string destinationProviderGuidIdentifier)
		{
			var query = _artifactIdByGuid.Create(destinationProviderGuidIdentifier);

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

		private static DestinationProviderDTO CreateDestinationProviderDTO(DestinationProvider result)
		{
			return new DestinationProviderDTO
			{
				ArtifactId = result.ArtifactId,
				Identifier = new Guid(result.Identifier),
				Name = result.Name
			};
		}

	}

}