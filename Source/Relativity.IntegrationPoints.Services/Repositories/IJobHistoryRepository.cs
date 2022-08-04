namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IJobHistoryRepository
    {
        JobHistorySummaryModel GetJobHistory(JobHistoryRequest request);
    }
}