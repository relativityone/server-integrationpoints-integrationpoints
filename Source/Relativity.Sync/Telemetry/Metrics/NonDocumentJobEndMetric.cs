namespace Relativity.Sync.Telemetry.Metrics
{
	internal class NonDocumentJobEndMetric : JobEndMetricBase<NonDocumentJobEndMetric>
	{
		[Metric(MetricType.PointInTimeString, TelemetryConstants.MetricIdentifiers.JOB_END_STATUS_NON_DOCUMENT_OBJECTS)]
		public override string JobEndStatus { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_FIELDS_TOTAL_REQUESTED)]
        public long? TotalAvailableFields { get; set; }

		[Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED)]
		public long? TotalMappedFields { get; set; }

        [Metric(MetricType.PointInTimeLong, TelemetryConstants.MetricIdentifiers.DATA_BYTES_METADATA_TRANSFERRED)]
        public long? BytesMetadataTransferred { get; set; }
	}
}
