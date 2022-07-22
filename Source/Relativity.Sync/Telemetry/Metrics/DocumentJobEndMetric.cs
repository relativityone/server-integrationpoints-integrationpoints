using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry.Metrics
{
    internal class DocumentJobEndMetric : JobEndMetricBase<DocumentJobEndMetric>
    {
        [Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NATIVES_AND_METADATA)]
        public override string JobEndStatus { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_REQUESTED)]
        public long? BytesNativesRequested { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED)]
        public long? BytesMetadataTransferred { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_NATIVES_TRANSFERRED)]
        public long? BytesNativesTransferred { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED)]
        public long? TotalMappedFields { get; set; }

        public ImportNativeFileCopyMode? NativeFileCopyMode { get; set; }
    }
}
