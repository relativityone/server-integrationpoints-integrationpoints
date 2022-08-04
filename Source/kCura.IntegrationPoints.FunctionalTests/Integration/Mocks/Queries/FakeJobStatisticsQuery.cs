using kCura.IntegrationPoints.Data.Queries;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries
{
    public class FakeJobStatisticsQuery : IJobStatisticsQuery
    {
        public int AlreadyTransferredItems { get; set; }
        public int AlreadyFailedItems { get; set; }

        public int TotalProcessedItems => AlreadyFailedItems + AlreadyTransferredItems;


        public JobStatistics UpdateAndRetrieveStats(string tableName, long jobId, JobStatistics stats, int workspaceID)
        {
            AlreadyTransferredItems += stats.Completed;
            AlreadyFailedItems += stats.ImportApiErrors;

            stats.Errored = AlreadyFailedItems;
            stats.ImportApiErrors = AlreadyFailedItems;
            stats.Completed = TotalProcessedItems;

            return stats;
        }
    }
}