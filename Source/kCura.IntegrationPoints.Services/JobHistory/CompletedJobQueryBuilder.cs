using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class CompletedJobQueryBuilder : ICompletedJobQueryBuilder
	{
		public Query<RDO> CreateQuery(string sortColumn, bool sortDescending)
		{
			return new Query<RDO>
			{
				ArtifactTypeGuid = new Guid(ObjectTypeGuids.JobHistory),
				Fields = FieldValue.AllFields,
				Sorts = new List<Sort>
				{
					new Sort
					{
						Direction = sortDescending ? SortEnum.Descending : SortEnum.Ascending,
						Field = GetSortColumn(sortColumn)
					}
				},
				Condition = new SingleChoiceCondition(new Guid(JobHistoryFieldGuids.JobStatus), SingleChoiceConditionEnum.AnyOfThese, new List<Guid>
				{
					JobStatusChoices.JobHistoryCompleted.Guids[0],
					JobStatusChoices.JobHistoryCompletedWithErrors.Guids[0]
				})
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