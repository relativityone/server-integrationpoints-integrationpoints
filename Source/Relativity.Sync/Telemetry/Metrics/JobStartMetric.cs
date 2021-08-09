namespace Relativity.Sync.Telemetry.Metrics
{
	internal sealed class JobStartMetric : MetricBase<JobStartMetric>
	{
		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_START_TYPE)]
		public string Type { get; set; }

		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE)]
		public string RetryType { get; set; }

		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.FLOW_TYPE)]
		public string FlowType { get; set; }
	}
}
