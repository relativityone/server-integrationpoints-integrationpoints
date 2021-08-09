using System.Collections.Generic;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal sealed class DocumentBatchEndMetric : BatchEndMetric<DocumentBatchEndMetric>
	{
		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_LESSTHAN1MB)]
		public double? AvgSizeLessThan1MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_LESSTHAN1MB)]
		public double? AvgTimeLessThan1MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN1AND10MB)]
		public double? AvgSizeLessBetween1and10MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN1AND10MB)]
		public double? AvgTimeLessBetween1and10MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN10AND20MB)]
		public double? AvgSizeLessBetween10and20MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN10AND20MB)]
		public double? AvgTimeLessBetween10and20MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_SIZE_OVER20MB)]
		public double? AvgSizeOver20MB { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_AVERAGE_TIME_OVER20MB)]
		public double? AvgTimeOver20MB { get; set; }

		[APMIgnoreMetric]
		public List<LongTextStreamStatistics> TopLongTexts { get; set; } = new List<LongTextStreamStatistics>();
	}
}