namespace Relativity.Sync.Telemetry.Metrics
{
	internal class JobEndMetric : IMetric
	{
		public string Application { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers._DATA_RECORDS_TRANSFERRED)]
		public long TotalRecordsTransferred { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers._DATA_RECORDS_FAILED)]
		public long TotalRecordsFailed { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers._DATA_RECORDS_TOTAL_REQUESTED)]
		public long TotalRecordsRequested { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers._DATA_BYTES_TOTAL_TRANSFERRED)]
		public long TotalBytesTransferred { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers._DATA_BYTES_NATIVES_REQUESTED)]
		public long TotalBytesNativesRequested { get; set; }

		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers._JOB_END_STATUS)]
		public string JobStatus { get; set; }


	}
}
