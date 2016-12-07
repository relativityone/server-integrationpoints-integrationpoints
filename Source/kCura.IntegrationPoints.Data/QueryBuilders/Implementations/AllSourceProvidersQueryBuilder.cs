using System;
using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
	public class AllSourceProvidersQueryBuilder : IAllSourceProvidersQueryBuilder
	{
		public Query<RDO> Create()
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.SourceProvider),
				Fields = new List<FieldValue>
				{
					new FieldValue(new Guid(SourceProviderFieldGuids.Name)),
					new FieldValue(new Guid(SourceProviderFieldGuids.Identifier))
				}
			};
		}
	}
}