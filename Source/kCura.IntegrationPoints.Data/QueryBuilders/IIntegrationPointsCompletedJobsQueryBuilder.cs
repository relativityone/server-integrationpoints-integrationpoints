using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IIntegrationPointsCompletedJobsQueryBuilder
	{
		Query<RDO> CreateQuery(string sortColumn, bool sortDescending, List<int> integrationPointArtifactIds);
	}
}