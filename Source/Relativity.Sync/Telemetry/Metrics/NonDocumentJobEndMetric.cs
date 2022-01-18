using Relativity.Sync.Configuration;

namespace Relativity.Sync.Telemetry.Metrics
{
	internal class NonDocumentJobEndMetric : JobEndMetricBase<NonDocumentJobEndMetric>
	{
		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NONDOCUMENTS)]
		public override string JobEndStatus { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED)]
		public long? TotalMappedFields { get; set; }
	}
}
