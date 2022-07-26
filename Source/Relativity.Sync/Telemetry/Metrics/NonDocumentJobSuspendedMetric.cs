namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class NonDocumentJobSuspendedMetric : MetricBase<NonDocumentJobSuspendedMetric>
    {
        [Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_SUSPENDED_STATUS_NON_DOCUMENT_OBJECTS)]
        public string JobSuspendedStatus { get; set; }
    }
}