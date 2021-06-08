using kCura.IntegrationPoints.Data.Queries;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
    public class FakeJobStatisticsQuery : IJobStatisticsQuery
    {
        public JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID)
        {
            return stats;
        }
    }
}