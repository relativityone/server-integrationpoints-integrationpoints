namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IJobHistoryRepository
	{
		JobHistorySummaryModel GetJobHistory(JobHistoryRequest request);
		JobHistorySummaryModel GetJobHistoryWithStatusCompleted(JobHistoryRequest request);
	}
}