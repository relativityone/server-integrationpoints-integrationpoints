using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

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
			JobHistoryError historyError = GetJobLevelError(jobHistoryId);
			if (historyError != null)
			{
				return historyError;
			}

			//var query = new Query<RDO>();
			//query.Fields = new List<FieldValue>
			//{
			//	new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType)),
			//	new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.Error))
			//};

			//var historyCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);
			//JobHistoryError historyError = this.GetJobLevelError(jobHistoryId);

			string historyCondition = CreateJobHistoryObjectCondition(jobHistoryId);
			string expectedChoiceGuids = string.Join(",", ErrorTypeChoices.JobHistoryErrorItem.Guids.Select(x => x.ToString()));
			string choiceJobItemCondition = $"'{JobHistoryErrorFields.ErrorType}' IN CHOICE [{expectedChoiceGuids}]";
			string condition = $"{historyCondition} AND {choiceJobItemCondition}";

			var query = new QueryRequest
			{
				Fields = GetFieldsToRetrieve(),
				Condition = condition
			};
			historyError = _service.RelativityObjectManager.Query<JobHistoryError>(query, 0, 1).Items.FirstOrDefault();

			//var choiceJobItemCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
			//ErrorTypeChoices.JobHistoryErrorItem.Guids);
			//query.Condition = new CompositeCondition(choiceJobItemCondition, CompositeConditionEnum.And, historyCondition);
			//historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();

			return historyError;
		}

		public virtual JobHistoryError GetJobLevelError(int jobHistoryId)
		{
			// TODO remove
			//var query = new Query<RDO>();
			//query.Fields = new List<FieldValue>
			//{
			//	new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.ErrorType)),
			//	new FieldValue(Guid.Parse(Data.JobHistoryErrorFieldGuids.Error))
			//};
			//query.Sorts = new List<Sort>
			//{ 
			//	new Sort
			//	{
			//		Field = "Artifact ID",
			//		Direction = SortEnum.Descending
			//	}
			//};

			//var historyCondition = new ObjectCondition(Guid.Parse(JobHistoryErrorFieldGuids.JobHistory), ObjectConditionEnum.EqualTo, jobHistoryId);
			//var choiceJobErrorCondition = new SingleChoiceCondition(Guid.Parse(JobHistoryErrorFieldGuids.ErrorType), SingleChoiceConditionEnum.AnyOfThese,
			//	ErrorTypeChoices.JobHistoryErrorJob.Guids);
			//query.Condition = new CompositeCondition(choiceJobErrorCondition, CompositeConditionEnum.And, historyCondition);
			//JobHistoryError historyError = _service.JobHistoryErrorLibrary.Query(query, 1).FirstOrDefault();
			//return historyError;

			string historyCondition = CreateJobHistoryObjectCondition(jobHistoryId);
			string expectedChoiceGuids = string.Join(",", ErrorTypeChoices.JobHistoryErrorJob.Guids.Select(x => x.ToString()));
			string choiceJobErrorCondition = $"'{JobHistoryErrorFields.ErrorType}' IN CHOICE [{expectedChoiceGuids}]";
			string condition = $"{historyCondition} AND {choiceJobErrorCondition}";

			var query = new QueryRequest
			{
				Fields = GetFieldsToRetrieve(),
				Sorts = GetSortByArtifactIdDescendingCondition().ToList(),
				Condition = condition
			};

			JobHistoryError historyError = _service.RelativityObjectManager.Query<JobHistoryError>(query, 0, 1).Items.FirstOrDefault();
			return historyError;
		}

		private string CreateJobHistoryObjectCondition(int jobHistoryId)
		{
			return $"'{JobHistoryErrorFields.JobHistory}' == OBJECT {jobHistoryId}";
		}

		private IEnumerable<FieldRef> GetFieldsToRetrieve()
		{
			yield return new FieldRef { Guid = Guid.Parse(JobHistoryErrorFieldGuids.ErrorType) };
			yield return new FieldRef { Guid = Guid.Parse(JobHistoryErrorFieldGuids.Error) };
		}

		private IEnumerable<Sort> GetSortByArtifactIdDescendingCondition()
		{
			yield return new Sort
			{
				Direction = SortEnum.Descending,
				FieldIdentifier = new FieldRef { Name = "Artifact ID" }
			};
		}
	}
}
