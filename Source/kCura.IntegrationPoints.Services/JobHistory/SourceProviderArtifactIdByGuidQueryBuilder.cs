using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class SourceProviderArtifactIdByGuidQueryBuilder
	{
		public Query<RDO> Create(string guid)
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.SourceProvider),
				Condition = new TextCondition(new Guid(SourceProviderFieldGuids.Identifier), TextConditionEnum.EqualTo, guid),
				Fields = new List<FieldValue>()
				{
					new FieldValue("Artifact ID")
				}
			};
		}
	}
}