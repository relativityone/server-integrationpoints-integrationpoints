namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages
{
	public class JobThroughputMessage : JobMessageBase
	{
		public double RecordsPerSecond { get; set; }
	}
}