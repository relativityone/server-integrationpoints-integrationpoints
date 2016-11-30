using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class DestinationProviderArtifactIdByGuidQueryBuilder
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