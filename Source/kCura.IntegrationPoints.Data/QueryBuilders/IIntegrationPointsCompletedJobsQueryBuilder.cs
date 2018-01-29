using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders
{
	public interface IIntegrationPointsCompletedJobsQueryBuilder
	{
		QueryRequest CreateQuery(string sortColumn, bool sortDescending, List<int> integrationPointArtifactIds);
	}
}