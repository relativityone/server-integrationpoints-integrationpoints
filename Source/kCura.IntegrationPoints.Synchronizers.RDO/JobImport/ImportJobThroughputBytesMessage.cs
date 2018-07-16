using System.Collections.Generic;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobThroughputBytesMessage : IMessage, IMetricMetadata
	{
		public string Provider { get; set; }
		public string CorrelationID { get; set; }
		public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }
		
		public double BytesPerSecond { get; set; }
	}
}