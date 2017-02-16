using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IRelativityIntegrationPointsRepository
	{
		List<Core.Models.IntegrationPointModel> RetrieveIntegrationPoints();
	}
}