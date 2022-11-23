using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class JobHistoryErrorQuery
    {
        private readonly IRelativityObjectManagerService _service;

        public JobHistoryErrorQuery(IRelativityObjectManagerService service)
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

            return historyError;
        }

        public virtual JobHistoryError GetJobLevelError(int jobHistoryId)
        {
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
