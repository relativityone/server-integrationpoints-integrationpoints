using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryService : IDeleteHistoryService
	{
		private readonly IRelativityObjectManagerFactory _objectManagerFactory;

		public DeleteHistoryService(IRelativityObjectManagerFactory objectManagerFactory)
		{
			_objectManagerFactory = objectManagerFactory;
		}

		public void DeleteHistoriesAssociatedWithIP(int workspaceId, int integrationPointId)
		{
			DeleteHistoriesAssociatedWithIPs(new List<int> {integrationPointId}, _objectManagerFactory.CreateRelativityObjectManager(workspaceId));
		}

		public void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId, IRelativityObjectManager objectManager)
		{
		    if (!integrationPointsId.Any())
		    {
		        return;
		    }

			var request = new QueryRequest
			{
				Condition = $"'ArtifactId' in [{string.Join(",", integrationPointsId)}]"

			};
			IList<Data.IntegrationPoint> integrationPoints = objectManager.Query<Data.IntegrationPoint>(request);

			// Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
			// We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
			// JobHistoryErrors will be now removed by Management Agent

			foreach (Data.IntegrationPoint integrationPoint in integrationPoints)
			{
				integrationPoint.JobHistory = null;
				objectManager.Update(integrationPoint);
			}
		}
	}
}