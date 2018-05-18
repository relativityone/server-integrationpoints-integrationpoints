namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages
{
	public class JobThroughputMessage : JobMessageBase
	{
		public double Throughput { get; set; }
	}
}