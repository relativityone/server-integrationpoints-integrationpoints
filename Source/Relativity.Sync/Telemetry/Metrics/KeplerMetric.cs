namespace Relativity.Sync.Telemetry.Metrics
{
	internal class KeplerMetric : PrefixedMetricBase
	{
		public ExecutionStatus? ExecutionStatus { get; set; }

		[Metric(MetricType.TimedOperation, TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX)]
		public double? Duration { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX)]
		public long? NumberOfHttpRetriesForSuccess { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX)]
		public long? NumberOfHttpRetriesForFailed { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX)]
		public long? AuthTokenExpirationCount { get; set; }

		public KeplerMetric(string prefix) : base(prefix)
		{
		}
	}
}
