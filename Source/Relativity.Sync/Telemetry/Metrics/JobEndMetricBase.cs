using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry.Metrics
{
    internal abstract class JobEndMetricBase<T> : MetricBase<T> where T: JobEndMetricBase<T>, new()
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

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_TOTAL_TRANSFERRED)]
        public long? BytesTransferred { get; set; }

        public DataSourceType? SourceType { get; set; }

        public DestinationLocationType? DestinationType { get; set; }

        public ImportOverwriteMode? OverwriteMode { get; set; }
    }
}
