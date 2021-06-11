namespace Relativity.Sync.Telemetry.Metrics
{
	internal sealed class JobResumeMetric : MetricBase<JobResumeMetric>
	{
		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_RESUME_TYPE)]
		public string Type { get; set; }

		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.RETRY_JOB_START_TYPE)]
		public string RetryType { get; set; }
	}
}