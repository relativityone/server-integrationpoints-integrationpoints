using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Data.Queries
{
	public class JobHistoryErrorQuery
	{
		private readonly IRSAPIService _service;
		public JobHistoryErrorQuery(IRSAPIService service)
		{
			_service = service;
		}

		public virtual JobHistoryError GetJobErrorFailedStatus(int jobHistoryId)
		{
			var query = new Query<RDO>();
			query.Fields = new List<FieldValue>
			{
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType))
			};

			var historyCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);

			var choiceJobErrorCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
				new List<Guid> { Guid.Parse("FA8BB625-05E6-4BF7-8573-012146BAF19B") });
			query.Condition = new CompositeCondition(choiceJobErrorCondition, CompositeConditionEnum.And, historyCondition);
			JobHistoryError historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();

			if (historyError == null)
			{
				var choiceJobItemCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
					new List<Guid> { Guid.Parse("9DDC4914-FEF3-401F-89B7-2967CD76714B") });
				query.Condition = new CompositeCondition(choiceJobItemCondition, CompositeConditionEnum.And, historyCondition);
				historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();
			}

			return historyError;
		}



	}
}
