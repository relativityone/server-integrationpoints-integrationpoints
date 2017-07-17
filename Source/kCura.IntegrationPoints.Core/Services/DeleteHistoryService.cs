using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryService
	{
		private readonly IRSAPIService _context;

		public DeleteHistoryService(IRSAPIService context)
		{
			_context = context;
		}

		public void DeleteHistoriesAssociatedWithIP(int integrationPointId)
		{
			DeleteHistoriesAssociatedWithIPs(new List<int> {integrationPointId});
		}

		public void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId)
		{
			var query = new Query<RDO>
			{
				Fields = new List<FieldValue>
				{
					new FieldValue(Guid.Parse(IntegrationPointFieldGuids.JobHistory))
				},
				Condition = new ObjectCondition("Artifact ID", ObjectConditionEnum.AnyOfThese, integrationPointsId)
			};
			var integrationPoints = _context.IntegrationPointLibrary.Query(query);

			// Since 9.4 release we're not deleting job history RDOs (they've being used by ECA Dashboard)
			// We're also not removing JobHistoryErrors as it was taking too long (SQL timeouts)
			// JobHistoryErrors will be now removed by Management Agent

			foreach (var integrationPoint in integrationPoints)
			{
				integrationPoint.JobHistory = null;
			}

			_context.IntegrationPointLibrary.Update(integrationPoints);
		}
	}
}