using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface ICompletedJobsHistoryRepository
	{
		IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoints(JobHistoryRequest request, List<int> integrationPointIds);
	}
}