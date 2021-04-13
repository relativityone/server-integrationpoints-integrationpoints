namespace Relativity.Sync.Telemetry.Metrics
{
	internal sealed class ImageJobSuspendedMetric : MetricBase<DocumentJobSuspendedMetric>
	{
		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_SUSPENDED_STATUS_IMAGES)]
		public string JobSuspendedStatus { get; set; }
	}
}