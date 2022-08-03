namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobThroughputBytesMessage : JobMessageBase
    {
        public double BytesPerSecond { get; set; }
    }
}