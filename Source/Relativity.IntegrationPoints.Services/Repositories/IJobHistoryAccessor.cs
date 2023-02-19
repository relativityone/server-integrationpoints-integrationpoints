namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IJobHistoryAccessor
    {
        JobHistorySummaryModel GetJobHistory(JobHistoryRequest request);
    }
}
