namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class DocumentJobSuspendedMetric : MetricBase<DocumentJobSuspendedMetric>
    {
        [Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_SUSPENDED_STATUS_NATIVES_AND_METADATA)]
        public string JobSuspendedStatus { get; set; }
    }
}