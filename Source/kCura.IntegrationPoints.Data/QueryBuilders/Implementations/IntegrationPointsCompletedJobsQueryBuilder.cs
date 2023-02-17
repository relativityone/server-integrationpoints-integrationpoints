using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.QueryBuilders.Implementations
{
    public class IntegrationPointsCompletedJobsQueryBuilder : IIntegrationPointsCompletedJobsQueryBuilder
    {
        public QueryRequest CreateQuery(string sortColumn, bool sortDescending, List<int> integrationPointArtifactIds)
        {
            return new QueryRequest
            {
                Sorts = CreateSort(sortColumn, sortDescending),
                Condition = CreateConditions(integrationPointArtifactIds)
            };
        }

        private string CreateConditions(List<int> integrationPointArtifactIds)
        {
            var jobHistoryCompletedCondition = CreateJobHistoryCompletedCondition();
            var integrationPointRelationCondition = CreateIntegrationPointRelationCondition(integrationPointArtifactIds);
            var atLeastOneDocumentTransferedCondition = CreateAtLeastOneDocumentTransferedCondition();

            string condition = $"{jobHistoryCompletedCondition} AND {integrationPointRelationCondition} AND {atLeastOneDocumentTransferedCondition}";

            return condition;
        }

        private string CreateAtLeastOneDocumentTransferedCondition()
        {
            return $"'{JobHistoryFields.ItemsTransferred}' > 0";
        }

        private string CreateIntegrationPointRelationCondition(List<int> integrationPointArtifactIds)
        {
            return $"'{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{string.Join(",", integrationPointArtifactIds)}]";
        }

        private string CreateJobHistoryCompletedCondition()
        {
            List<Guid> choices = new List<Guid>
            {
                JobStatusChoices.JobHistoryCompleted.Guids[0],
                JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]
            };
            return $"'{JobHistoryFields.JobStatus}' IN CHOICE [{string.Join(",", choices)}]";
        }

        private List<Sort> CreateSort(string sortColumn, bool sortDescending)
        {
            return new List<Sort>
            {
                new Sort
                {
                    Direction = sortDescending ? SortEnum.Descending : SortEnum.Ascending,
                    FieldIdentifier = GetSortColumn(sortColumn)
                }
            };
        }

        private FieldRef GetSortColumn(string sortColumnName)
        {
            FieldRef sortColumn = new FieldRef()
            {
                Name = string.IsNullOrWhiteSpace(sortColumnName)
                    ? nameof(JobHistory.DestinationWorkspace)
                    : sortColumnName
            };

            return sortColumn;
        }
    }
}
