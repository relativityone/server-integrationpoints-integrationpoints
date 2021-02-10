namespace Relativity.Sync.Telemetry.Metrics
{
	internal class StreamRetryMetric : MetricBase
	{
		[Metric(MetricType.Counter, TelemetryConstants.MetricIdentifiers.LONG_TEXT_STREAM_RETRY_COUNT)]
		public Counter? RetryCounter { get; set; }
	}
}
