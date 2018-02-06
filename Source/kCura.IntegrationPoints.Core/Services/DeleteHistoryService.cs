using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryService : IDeleteHistoryService
	{
		private readonly IRSAPIServiceFactory _rsapiServiceFactory;

		public DeleteHistoryService(IRSAPIServiceFactory rsapiServiceFactory)
		{
			_rsapiServiceFactory = rsapiServiceFactory;
		}

		public void DeleteHistoriesAssociatedWithIP(int workspaceId, int integrationPointId)
		{
			DeleteHistoriesAssociatedWithIPs(new List<int> {integrationPointId}, _rsapiServiceFactory.Create(workspaceId));
		}

		public void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId, IRSAPIService rsapiService)
		{

			QueryRequest request = new QueryRequest()
			{
				Condition = $"'ArtifactId' in [{String.Join(",", integrationPointsId)}]"

			};
			var integrationPoints = rsapiService.RelativityObjectManager.Query<Data.IntegrationPoint>(request);

			// Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
			// We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
			// JobHistoryErrors will be now removed by Management Agent

			foreach (var integrationPoint in integrationPoints)
			{
				integrationPoint.JobHistory = null;
				rsapiService.RelativityObjectManager.Update(integrationPoint);
			}
		}
	}
}