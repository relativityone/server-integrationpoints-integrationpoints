using Microsoft.Azure.Cosmos.Table;

namespace Relativity.Sync.Tests.Performance.Helpers
{
    public class TestResult : TableEntity
    {
        public double Duration { get; set; }

        public TestResult()
        {
        }

        public TestResult(string testName, string buildId)
        {
            PartitionKey = testName;
            RowKey = buildId;
        }
    }
}
