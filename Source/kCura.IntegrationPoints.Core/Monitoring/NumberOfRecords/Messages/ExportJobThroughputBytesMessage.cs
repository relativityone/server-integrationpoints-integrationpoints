namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages
{
	public class ExportJobThroughputBytesMessage : JobMessageBase
	{
		public double BytesPerSecond { get; set; }
	}
}