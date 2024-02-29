namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class SourceWorkspaceTagMetric : MetricBase<SourceWorkspaceTagMetric>
    {
        [Metric(MetricType.TimedOperation, TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_DESTINATION_UPDATE_TIME)]
        public double? DestinationUpdateTime { get; set; }

        [Metric(MetricType.GaugeOperation, TelemetryConstants.MetricIdentifiers.TAG_DOCUMENTS_DESTINATION_UPDATE_COUNT)]
        public long? DestinationUpdateCount { get; set; }

        public string UnitOfMeasure { get; set; }

        public int? BatchSize { get; set; }
    }
}
