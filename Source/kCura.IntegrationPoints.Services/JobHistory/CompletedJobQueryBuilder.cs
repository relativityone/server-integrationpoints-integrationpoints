using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class CompletedJobQueryBuilder : ICompletedJobQueryBuilder
	{
		public Query<RDO> CreateQuery(string sortColumn, bool sortDescending, List<int> integrationPointArtifactIds)
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Fields = FieldValue.AllFields,
				Sorts = CreateSort(sortColumn, sortDescending),
				Condition = CreateConditions(integrationPointArtifactIds)
			};
		}

		private Condition CreateConditions(List<int> integrationPointArtifactIds)
		{
			var jobHistoryCompletedCondition = CreateJobHistoryCompletedCondition();
			var integrationPointRelationCondition = CreateIntegrationPointRelationCondition(integrationPointArtifactIds);
			var atLeastOneDocumentTransferedCondition = CreateAtLeastOneDocumentTransferedCondition();

			var andConditions = new CompositeCondition(jobHistoryCompletedCondition, CompositeConditionEnum.And, integrationPointRelationCondition);
			return new CompositeCondition(andConditions, CompositeConditionEnum.And, atLeastOneDocumentTransferedCondition);
		}

		private Condition CreateAtLeastOneDocumentTransferedCondition()
		{
			return new WholeNumberCondition(new Guid(JobHistoryFieldGuids.ItemsTransferred), NumericConditionEnum.GreaterThan, 0);
		}

		private Condition CreateIntegrationPointRelationCondition(List<int> integrationPointArtifactIds)
		{
			return new ObjectsCondition(new Guid(JobHistoryFieldGuids.IntegrationPoint), ObjectsConditionEnum.AnyOfThese, integrationPointArtifactIds);
		}

		private Condition CreateJobHistoryCompletedCondition()
		{
			return new SingleChoiceCondition(new Guid(JobHistoryFieldGuids.JobStatus), SingleChoiceConditionEnum.AnyOfThese, new List<Guid>
			{
				JobStatusChoices.JobHistoryCompleted.Guids[0],
				JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]
			});
		}

		private List<Sort> CreateSort(string sortColumn, bool sortDescending)
		{
			return new List<Sort>
			{
				new Sort
				{
					Direction = sortDescending ? SortEnum.Descending : SortEnum.Ascending,
					Field = GetSortColumn(sortColumn)
				}
			};
		}

		private string GetSortColumn(string sortColumnName)
		{
			string sortColumn = string.IsNullOrEmpty(sortColumnName)
				? nameof(JobHistoryModel.DestinationWorkspace)
				: sortColumnName;

			return sortColumn;
		}
	}
}