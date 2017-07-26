using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IDeleteHistoryService
	{
		void DeleteHistoriesAssociatedWithIP(int workspaceId, int integrationPointId);
		void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId, IRSAPIService rsapiService);
	}
}