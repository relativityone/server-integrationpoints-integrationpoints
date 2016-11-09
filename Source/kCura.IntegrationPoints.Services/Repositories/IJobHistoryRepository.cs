namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IJobHistoryRepository
	{
		JobHistorySummaryModel GetJobHistory(JobHistoryRequest request);
	}
}