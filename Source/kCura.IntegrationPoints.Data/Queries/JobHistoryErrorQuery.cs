using System;
using System.Collections.Generic;
using System.Linq;
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
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType)),
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.Error))
			};

			var historyCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);
			JobHistoryError historyError = this.GetJobLevelError(jobHistoryId);

			if (historyError == null)
			{
				var choiceJobItemCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
				ErrorTypeChoices.JobHistoryErrorItem.Guids);
				query.Condition = new CompositeCondition(choiceJobItemCondition, CompositeConditionEnum.And, historyCondition);
				historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();
			}

			return historyError;
		}

		public virtual JobHistoryError GetJobLevelError(int jobHistoryId)
		{

			var query = new Query<RDO>();
			query.Fields = new List<FieldValue>
			{
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType)),
				new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.Error))
			};
			query.Sorts = new List<Sort>
			{ 
				new Sort
				{
					Field = "Artifact ID",
					Direction = SortEnum.Descending
				}
			};

			var historyCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);
			var choiceJobErrorCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
				ErrorTypeChoices.JobHistoryErrorJob.Guids);
			query.Condition = new CompositeCondition(choiceJobErrorCondition, CompositeConditionEnum.And, historyCondition);
			JobHistoryError historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();
			return historyError;

		}

	}
}
