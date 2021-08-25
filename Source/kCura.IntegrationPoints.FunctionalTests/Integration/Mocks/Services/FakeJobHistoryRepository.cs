using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    class FakeJobHistoryRepository : IJobHistoryRepository
    {
        public JobHistorySummaryModel GetJobHistory(JobHistoryRequest request)
        {
            JobHistorySummaryModel expectedJobHistorySummaryModel = new JobHistorySummaryModel
            {
                Data = new[] { new JobHistoryModel() },
                TotalAvailable = 10,
                TotalDocumentsPushed = 20
            };

            return expectedJobHistorySummaryModel;
        }
    }
}
