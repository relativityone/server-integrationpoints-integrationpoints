using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class GetRecentJobHistory
	{
		private readonly IRSAPIService _service;
		public GetRecentJobHistory(IRSAPIService service)
		{
			_service = service;
		}

		public virtual JobHistoryError Execute(int jobHistoryId)
		{
			var query = new Query<RDO>();
			query.Fields = new List<FieldValue>
			{
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType))
			};
			query.Sorts = new List<Sort>
			{
				new Sort{ Field = "Artifact ID", Direction = SortEnum.Descending}
			};
			query.Condition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);

			var mostRecentJob = _service.JobHistoryErrorLibrary.Query(query).FirstOrDefault();

			return mostRecentJob;
		}



	}
}
