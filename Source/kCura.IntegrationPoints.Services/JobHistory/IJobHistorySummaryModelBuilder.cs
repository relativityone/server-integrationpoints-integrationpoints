using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface IJobHistorySummaryModelBuilder
	{
		JobHistorySummaryModel Create(int page, int pageSize, IList<JobHistoryModel> jobHistories);
	}
}