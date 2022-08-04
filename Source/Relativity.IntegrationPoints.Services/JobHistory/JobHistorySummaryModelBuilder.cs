using System.Collections.Generic;
using System.Linq;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public class JobHistorySummaryModelBuilder : IJobHistorySummaryModelBuilder
    {
        public JobHistorySummaryModel Create(int page, int pageSize, IList<JobHistoryModel> jobHistories)
        {
            var jobHistorySummary = new JobHistorySummaryModel();
            var jobHistoryModels = new List<JobHistoryModel>();

            var start = page*pageSize;
            var end = start + pageSize;

            for (int i = start; (i < end) && (i < jobHistories.Count); i++)
            {
                var history = jobHistories[i];
                jobHistoryModels.Add(history);
            }

            jobHistorySummary.Data = jobHistoryModels.ToArray();
            jobHistorySummary.TotalAvailable = jobHistories.Count();
            jobHistorySummary.TotalDocumentsPushed = jobHistories.Sum(x => x.ItemsTransferred);

            return jobHistorySummary;
        }
    }
}