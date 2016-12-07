using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class AllIntegrationPointTypesQueryBuilder : IAllIntegrationPointTypesQueryBuilder
	{
		public Query<RDO> Create()
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.IntegrationPointType),
				Fields = new List<FieldValue>
				{
					new FieldValue(new Guid(IntegrationPointTypeFieldGuids.Name)),
					new FieldValue(new Guid(IntegrationPointTypeFieldGuids.Identifier))
				}
			};
		}
	}
}