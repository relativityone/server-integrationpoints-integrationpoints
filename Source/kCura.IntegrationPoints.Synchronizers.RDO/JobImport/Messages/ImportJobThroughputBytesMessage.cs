namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobThroughputBytesMessage : ImportJobMessageBase
	{
		public double BytesPerSecond { get; set; }
	}
}