using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface ICompletedJobQueryBuilder
	{
		Query<RDO> CreateQuery(string sortColumn, bool sortDescending, List<int> integrationPointArtifactIds);
	}
}