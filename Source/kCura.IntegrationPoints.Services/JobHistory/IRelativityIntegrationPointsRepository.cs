using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IRelativityIntegrationPointsRepository
	{
		List<Data.IntegrationPoint> RetrieveRelativityIntegrationPoints(int workspaceId);
	}
}