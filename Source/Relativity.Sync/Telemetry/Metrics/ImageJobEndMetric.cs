using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry.Metrics
{
    internal class ImageJobEndMetric : JobEndMetricBase<ImageJobEndMetric>
    {
        [Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_IMAGES)]
        public override string JobEndStatus { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_REQUESTED)]
        public long? BytesImagesRequested { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_IMAGES_TRANSFERRED)]
        public long? BytesImagesTransferred { get; set; }

        public ImportImageFileCopyMode? ImageFileCopyMode { get; set; }
    }
}
