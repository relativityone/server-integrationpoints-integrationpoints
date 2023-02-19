namespace kCura.IntegrationPoint.Tests.Core.Models
{
    public class JobHistoryModel
    {
        public string JobStatus { get; set; }

        public int ItemsTransferred { get; set; }

        public int TotalItems { get; set; }

        public int ItemsWithErrors { get; set; }
    }
}
