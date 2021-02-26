namespace Relativity.Sync.Telemetry.Metrics
{
	internal class BatchEndMetric : MetricBase<BatchEndMetric>
	{
		public long? TotalRecordsRequested { get; set; }
		
		public long? TotalRecordsTransferred { get; set; }

		public long? TotalRecordsFailed { get; set; }

		public long? TotalRecordsTagged { get; set; }
		
		public long? BytesNativesTransferred { get; set; }
		
		public long? BytesMetadataTransferred { get; set; }

		public long? BytesTransferred { get; set; }
		
		public double? BatchTotalTime { get; set; }
		
		public double? BatchImportAPITime { get; set; }
	}
}