namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages
{
	public class ExportJobThroughputBytesMessage : JobApmMessageBase
	{
		public double BytesPerSecond { get; set; }
	}
}