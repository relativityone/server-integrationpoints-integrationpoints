using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class AllDestinationProvidersWithNameQueryBuilder : IAllDestinationProvidersWithNameQueryBuilder
	{
		public Query<RDO> Create()
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.DestinationProvider),
				Fields = new List<FieldValue>
				{
					new FieldValue(new Guid(DestinationProviderFieldGuids.Name)),
					new FieldValue(new Guid(DestinationProviderFieldGuids.Identifier))
				}
			};
		}
	}
}