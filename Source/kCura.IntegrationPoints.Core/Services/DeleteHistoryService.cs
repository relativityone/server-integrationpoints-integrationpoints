using System;
using System.Collections.Generic;
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

			QueryRequest request = new QueryRequest
			{
				Condition = $"'ArtifactId' in [{String.Join(",", integrationPointsId)}]"

			};
			var integrationPoints = objectManager.Query<Data.IntegrationPoint>(request);

			// Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
			// We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
			// JobHistoryErrors will be now removed by Management Agent

			foreach (var integrationPoint in integrationPoints)
			{
				integrationPoint.JobHistory = null;
				objectManager.Update(integrationPoint);
			}
		}
	}
}