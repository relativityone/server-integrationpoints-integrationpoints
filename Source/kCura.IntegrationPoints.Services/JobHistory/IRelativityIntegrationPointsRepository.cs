using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IRelativityIntegrationPointsRepository
	{
		List<int> RetrieveRelativityIntegrationPointsIds(int workspaceId);
	}
}