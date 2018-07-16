using System.Collections.Generic;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public abstract class JobMessageBase : IMessage, IMetricMetadata
	{
		public string Provider { get; set; }

		public string CorrelationID { get; set; }
		public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }
	}
}