using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IDeleteHistoryService
	{
		void DeleteHistoriesAssociatedWithIP(int workspaceID, int integrationPointID);
		void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsIDs, IRelativityObjectManager objectManager);
	}
}