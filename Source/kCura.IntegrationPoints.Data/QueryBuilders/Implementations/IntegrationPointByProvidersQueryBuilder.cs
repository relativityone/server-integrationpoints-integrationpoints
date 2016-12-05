using System;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class IntegrationPointByProvidersQueryBuilder : IIntegrationPointByProvidersQueryBuilder
	{
		public Query<RDO> CreateQuery(int sourceProviderArtifactId, int destinationProviderArtifactId)
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.IntegrationPoint),
				Fields = FieldValue.AllFields,
				Condition = CreateRelativityCondition(sourceProviderArtifactId, destinationProviderArtifactId)
			};
		}

		private Condition CreateRelativityCondition(int sourceProviderArtifactId, int destinationProviderArtifactId)
		{
			var sourceProviderCondition = new ObjectCondition(new Guid(IntegrationPointFieldGuids.SourceProvider), ObjectConditionEnum.EqualTo, sourceProviderArtifactId);
			var destinationProviderCondition = new ObjectCondition(new Guid(IntegrationPointFieldGuids.DestinationProvider), ObjectConditionEnum.EqualTo,
				destinationProviderArtifactId);

			return new CompositeCondition(sourceProviderCondition, CompositeConditionEnum.And, destinationProviderCondition);
		}
	}
}