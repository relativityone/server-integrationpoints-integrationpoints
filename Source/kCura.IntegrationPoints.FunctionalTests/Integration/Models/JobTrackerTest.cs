namespace Relativity.IntegrationPoints.Tests.Integration.Models
{
    public class JobTrackerTest
    {
        public long JobId { get; set; }

        public int TotalRecords { get; set; }

        public int ErrorRecords { get; set; }

        public int ImportApiErrors { get; set; }

        public bool Completed { get; set; }
    }
}
