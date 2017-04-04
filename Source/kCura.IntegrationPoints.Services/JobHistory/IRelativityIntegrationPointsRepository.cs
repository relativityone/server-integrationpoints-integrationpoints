using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IRelativityIntegrationPointsRepository
	{
		List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints();
	}
}