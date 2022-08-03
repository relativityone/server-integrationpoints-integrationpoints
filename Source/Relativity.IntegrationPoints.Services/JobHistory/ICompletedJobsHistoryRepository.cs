using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services.JobHistory
{
    public interface ICompletedJobsHistoryRepository
    {
        IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoints(JobHistoryRequest request, List<int> integrationPointIds);
        IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoint(JobHistoryRequest request, int integrationPointId);
    }
}