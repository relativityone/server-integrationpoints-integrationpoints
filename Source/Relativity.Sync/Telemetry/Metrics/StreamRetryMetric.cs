namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class StreamRetryMetric : MetricBase<StreamRetryMetric>
    {
        [Metric(MetricType.Counter, TelemetryConstants.MetricIdentifiers.LONG_TEXT_STREAM_RETRY_COUNT)]
        public Counter? RetryCounter { get; set; }
    }
}
