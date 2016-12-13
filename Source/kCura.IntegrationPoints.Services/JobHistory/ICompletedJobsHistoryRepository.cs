using System.Collections.Generic;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public interface ICompletedJobsHistoryRepository
	{
		IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoints(JobHistoryRequest request, List<Data.IntegrationPoint> integrationPoints);
	}
}