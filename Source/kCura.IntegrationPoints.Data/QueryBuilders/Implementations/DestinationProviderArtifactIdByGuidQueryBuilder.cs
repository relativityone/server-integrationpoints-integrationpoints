using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class DestinationProviderArtifactIdByGuidQueryBuilder : IDestinationProviderArtifactIdByGuidQueryBuilder
	{
		public Query<RDO> Create(string guid)
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.DestinationProvider),
				Condition = new TextCondition(new Guid(DestinationProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, guid),
				Fields = new List<FieldValue>
				{
					new FieldValue("Artifact ID")
				}
			};
		}
	}
}