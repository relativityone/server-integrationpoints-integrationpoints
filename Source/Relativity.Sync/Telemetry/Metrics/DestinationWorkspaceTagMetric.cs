namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class DestinationWorkspaceTagMetric : MetricBase<DestinationWorkspaceTagMetric>
    {
        [Metric(MetricType.TimedOperation, TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_SOURCE_UPDATE_TIME)]
        public double? SourceUpdateTime { get; set; }

        [Metric(MetricType.GaugeOperation, TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_SOURCE_UPDATE_COUNT)]
        public long? SourceUpdateCount { get; set; }

        public string UnitOfMeasure { get; set; }

        public int? BatchSize { get; set; }
    }
}
