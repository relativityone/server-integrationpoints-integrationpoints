using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobThroughputBytesMessage : IMessage
	{
		public string Provider { get; set; }
		public double BytesPerSecond { get; set; }
	}
}