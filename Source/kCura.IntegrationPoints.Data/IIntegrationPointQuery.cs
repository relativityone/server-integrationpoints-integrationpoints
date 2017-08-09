using System.Collections.Generic;

namespace kCura.IntegrationPoints.Data
{
	public interface IIntegrationPointQuery
	{
		IList<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds);
		IList<IntegrationPoint> GetAllIntegrationPoints();
		IList<IntegrationPoint> GetAllIntegrationPointsProfileWithBasicColumns();
	}
}
