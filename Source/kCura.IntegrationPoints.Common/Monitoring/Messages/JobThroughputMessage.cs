namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobThroughputMessage : JobMessageBase
    {
        public double RecordsPerSecond { get; set; }
    }
}