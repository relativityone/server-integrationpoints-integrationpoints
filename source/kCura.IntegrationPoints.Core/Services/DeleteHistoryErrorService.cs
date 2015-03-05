using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public class DeleteHistoryErrorService :IDeleteHistoryErrorService
	{
		private readonly IRSAPIService _context;
		public DeleteHistoryErrorService(IRSAPIService context)
		{
			_context = context;
		}

		public void DeleteErrorAssociatedWithHistory(int historyId)
		{
			DeleteErrorAssociatedWithHistories(new List<int>() { historyId });
		}

		public  void DeleteErrorAssociatedWithHistories(List<int> historiesId)
		{

			var qry = new Query<Relativity.Client.DTOs.RDO>();
			qry.Fields = new List<FieldValue>()
				{
					new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.Name))
				};
			qry.Condition = new ObjectCondition(Data.JobHistoryErrorFields.JobHistory, ObjectConditionEnum.AnyOfThese, historiesId);
			var result = _context.JobHistoryErrorLibrary.Query(qry);
			var allJobHistoryError = result.Select(jobHistoryError => jobHistoryError.ArtifactId).ToList();

			_context.JobHistoryErrorLibrary.Delete(allJobHistoryError);

		}
	}
}
