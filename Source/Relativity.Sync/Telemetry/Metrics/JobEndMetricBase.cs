namespace Relativity.Sync.Telemetry.Metrics
{
	internal abstract class JobEndMetricBase : MetricBase
	{
		public abstract string JobEndStatus { get; set; }

		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.RETRY_JOB_END_STATUS)]
		public string RetryJobEndStatus { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED)]
		public long? TotalRecordsTransferred { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TAGGED)]
		public long? TotalRecordsTagged { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED)]
		public long? TotalRecordsFailed { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED)]
		public long? TotalRecordsRequested { get; set; }
	}
}
