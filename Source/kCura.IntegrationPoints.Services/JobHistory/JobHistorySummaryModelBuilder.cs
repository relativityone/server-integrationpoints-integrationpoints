using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class JobHistorySummaryModelBuilder : IJobHistorySummaryModelBuilder
	{
		public JobHistorySummaryModel Create(int page, int pageSize, IList<Data.JobHistory> jobHistories)
		{
			var jobHistorySummary = new JobHistorySummaryModel();
			var jobHistoryModels = new List<JobHistoryModel>();

			var start = page*pageSize;
			var end = start + pageSize;

			for (int i = start; (i < end) && (i < jobHistories.Count); i++)
			{
				var history = jobHistories[i];
				var jobHistory = new JobHistoryModel
				{
					ItemsTransferred = history.ItemsTransferred ?? 0,
					EndTimeUTC = history.EndTimeUTC.GetValueOrDefault(),
					DestinationWorkspace = history.DestinationWorkspace
				};
				jobHistoryModels.Add(jobHistory);
			}

			jobHistorySummary.Data = jobHistoryModels.ToArray();
			jobHistorySummary.TotalAvailable = jobHistories.Count();
			jobHistorySummary.TotalDocumentsPushed = jobHistories.Sum(x => x.ItemsTransferred ?? 0);

			return jobHistorySummary;
		}
	}
}