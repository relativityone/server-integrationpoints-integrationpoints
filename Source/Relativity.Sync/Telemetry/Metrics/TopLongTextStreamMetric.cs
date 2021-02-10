namespace Relativity.Sync.Telemetry.Metrics
{
	internal class TopLongTextStreamMetric : MetricBase
	{
		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_SIZE)]
		public double? LongTextStreamSize { get; set; }

		[Metric(MetricType.PointInTimeDouble, TelemetryConstants.MetricIdentifiers.DATA_LONGTEXT_STREAM_LARGEST_TIME)]
		public double? LongTextStreamTime { get; set; }
	}
}
