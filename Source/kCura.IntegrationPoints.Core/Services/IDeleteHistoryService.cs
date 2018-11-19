using System.Collections.Generic;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IDeleteHistoryService
	{
		void DeleteHistoriesAssociatedWithIP(int workspaceId, int integrationPointId);
		void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId, IRelativityObjectManager objectManager);
	}
}