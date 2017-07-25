using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

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
			var query = new Query<RDO>
			{
				Fields = new List<FieldValue>
				{
					new FieldValue(Guid.Parse(IntegrationPointFieldGuids.JobHistory))
				},
				Condition = new ObjectCondition("Artifact ID", ObjectConditionEnum.AnyOfThese, integrationPointsId)
			};
			var integrationPoints = rsapiService.IntegrationPointLibrary.Query(query);

			// Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
			// We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
			// JobHistoryErrors will be now removed by Management Agent

			foreach (var integrationPoint in integrationPoints)
			{
				integrationPoint.JobHistory = null;
			}

			rsapiService.IntegrationPointLibrary.Update(integrationPoints);
		}
	}
}