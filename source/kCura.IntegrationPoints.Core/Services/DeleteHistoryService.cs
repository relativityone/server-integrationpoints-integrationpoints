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

			var qry = new Query<Relativity.Client.DTOs.RDO>();
			qry.Fields = new List<FieldValue>()
				{
					new FieldValue(Guid.Parse(Data.IntegrationPointFieldGuids.JobHistory))
				};
			qry.Condition = new ObjectCondition("Artifact ID", ObjectConditionEnum.AnyOfThese, integrationPointsId);
			var result = _context.IntegrationPointLibrary.Query(qry);
			var allJobHistory = result.SelectMany(integrationPoint => integrationPoint.JobHistory).ToList();

			foreach (var integrationPoint in result)
			{
				integrationPoint.JobHistory = null;
			}
			_deleteError.DeleteErrorAssociatedWithHistories(allJobHistory);
			

			_context.IntegrationPointLibrary.Update(result);
			_context.JobHistoryLibrary.Delete(allJobHistory);
		}
	}
}
