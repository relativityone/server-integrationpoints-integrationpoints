using kCura.IntegrationPoints.Data.Queries;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
    public class FakeJobStatisticsQuery : IJobStatisticsQuery
    {
        public int AlreadyTransferredItems { get; set; }
        public int AlreadyFailedItems { get; set; }

        public JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID)
        {
	        stats.Completed += AlreadyFailedItems;
	        stats.Errored += AlreadyFailedItems;
            return stats;
        }
    }
}