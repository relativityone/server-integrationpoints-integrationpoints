using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class SourceProviderRepository : ISourceProviderRepository
	{
		private readonly ISourceProviderArtifactIdByGuidQueryBuilder _artifactIdByGuid = new SourceProviderArtifactIdByGuidQueryBuilder();
		private readonly IRelativityObjectManager _relativityObjectManager;

		public SourceProviderRepository(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		public SourceProviderDTO Read(int artifactId)
		{
			var nameFieldRef = new FieldRef { Name = SourceProviderFields.Name };
			var identifierFieldRef = new FieldRef { Name = SourceProviderFields.Identifier };
			var queryRequest = new QueryRequest
			{
				Condition = $"'{Domain.Constants.SOURCEPROVIDER_ARTIFACTID_FIELD_NAME}' == {artifactId}",
				Fields = new List<FieldRef> { nameFieldRef, identifierFieldRef },
				ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.SourceProvider) }
			};
			List<SourceProvider> queryResults;
			try
			{
				queryResults = _relativityObjectManager.Query<SourceProvider>(queryRequest);
				return CreateSourceProviderDTO(queryResults.Single());
			}
			catch (Exception e)
			{
				throw new IntegrationPointsException($"Failed to retrieve Source Provider for artifact Id: {artifactId}", e);
			}
		}

		public int GetArtifactIdFromSourceProviderTypeGuidIdentifier(string sourceProviderGuidIdentifier)
		{
			var query = _artifactIdByGuid.Create(sourceProviderGuidIdentifier);

			List<RelativityObject> queryResults;
			try
			{
				queryResults = _relativityObjectManager.Query(query);
				return queryResults.Single().ArtifactID;
			}
			catch (Exception e)
			{
				throw new IntegrationPointsException($"Failed to retrieve Source Provider Artifact Id for guid: {sourceProviderGuidIdentifier}", e);
			}
		}

		private static SourceProviderDTO CreateSourceProviderDTO(SourceProvider result)
		{
			return new SourceProviderDTO
			{
				ArtifactId = result.ArtifactId,
				Name = result.Name,
				Identifier = new Guid(result.Identifier)
			};
		}
	}
}