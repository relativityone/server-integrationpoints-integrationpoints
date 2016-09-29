using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryService
	{
		private readonly IRSAPIService _context;
		private readonly IDeleteHistoryErrorService _deleteError;
		public DeleteHistoryService(IRSAPIService context,IDeleteHistoryErrorService deleteHistoryErrorService)
		{
			_context = context;
			_deleteError = deleteHistoryErrorService;
		}

		public void DeleteHistoriesAssociatedWithIP(int integrationPointId)
		{
			DeleteHistoriesAssociatedWithIPs(new List<int>() { integrationPointId });
		}

		public void DeleteHistoriesAssociatedWithIPs(List<int> integrationPointsId)
		{
			var qry = new Query<Relativity.Client.DTOs.RDO>
			{
				Fields = new List<FieldValue>()
				{
					new FieldValue(Guid.Parse(Data.IntegrationPointFieldGuids.JobHistory))
				},
				Condition = new ObjectCondition("Artifact ID", ObjectConditionEnum.AnyOfThese, integrationPointsId)
			};
			var result = _context.IntegrationPointLibrary.Query(qry);
			var allJobHistory = result.SelectMany(integrationPoint => integrationPoint.JobHistory).ToList();

			foreach (var integrationPoint in result)
			{
				integrationPoint.JobHistory = null;
			}

			if (allJobHistory.Any())
			{
				_deleteError.DeleteErrorAssociatedWithHistories(allJobHistory);
			}

			_context.IntegrationPointLibrary.Update(result);

			// For the 9.4 release we are not deleting job history RDOs but we are deleting the job history errors RDOs - Dan Nelson 6/24/16
			//_context.JobHistoryLibrary.Delete(allJobHistory);
		}
	}
}
