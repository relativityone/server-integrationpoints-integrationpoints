namespace kCura.IntegrationPoints.Data.Queries
{
    public interface IJobStatisticsQuery
    {
        JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID);
    }
}