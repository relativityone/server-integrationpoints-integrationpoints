using System.Collections.Generic;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public abstract class ApmMessageBase : JobMessageBase, IMetricMetadata
	{
		public string CorellationID { get; set; }
		public Dictionary<string, object> CustomData { get; set; }
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }
	}
}