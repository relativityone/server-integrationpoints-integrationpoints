using System;

namespace Relativity.Sync.Telemetry
{
	internal class SumMetric
	{
		public MetricType Type { get; set; }

		public string Bucket { get; set; }

		public object Value { get; set; }

		public Guid WorkspaceGuid { get; set; }

		public string WorkflowId { get; set; }
	}
}
