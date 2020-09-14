namespace Relativity.Sync.Telemetry
{
	internal static class TelemetryConstants
	{
		private const string _SYNC_METRIC_PREPEND = "Relativity.Sync";
		private const string _INTEGRATION_POINTS_METRIC_PREPEND = "Relativity.IntegrationPoints";

		public const string SYNC_TELEMETRY_CATEGORY = "Relativity Sync";
		public const string INTEGRATION_POINTS_TELEMETRY_CATEGORY = "Integration Points";

		public const string PROVIDER_NAME = "Sync";

		public static class MetricIdentifiers
		{
			public const string KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX = "Duration";
			public const string KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX = "Success";
			public const string KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX = "Failed";
			public const string KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX = "AuthRefresh";

			public static readonly string JOB_START_TYPE = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Job.Start.Type";
			public static readonly string JOB_END_STATUS = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Job.End.Status";

			public static readonly string RETRY_JOB_START_TYPE = $"{_SYNC_METRIC_PREPEND}.Retry.Job.Start.Type";
			public static readonly string RETRY_JOB_END_STATUS = $"{_SYNC_METRIC_PREPEND}.Retry.Job.End.Status";

			public static readonly string DATA_BYTES_METADATA_TRANSFERRED = $"{_SYNC_METRIC_PREPEND}.Data.Bytes.MetadataTransferred";
			public static readonly string DATA_BYTES_TOTAL_TRANSFERRED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Bytes.TotalTransferred";
			public static readonly string DATA_BYTES_NATIVES_REQUESTED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Bytes.NativesRequested";
			public static readonly string DATA_RECORDS_TRANSFERRED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Records.Transferred";
			public static readonly string DATA_RECORDS_FAILED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Records.Failed";
			public static readonly string DATA_RECORDS_TOTAL_REQUESTED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Records.TotalRequested";
			public static readonly string DATA_FIELDS_MAPPED = $"{_INTEGRATION_POINTS_METRIC_PREPEND}.Data.Fields.Mapped";

			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_LESSTHAN1MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageSize.LessThan1MB";
			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_TIME_LESSTHAN1MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageTime.LessThan1MB";

			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN1AND10MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageSize.Between1And10MB";
			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN1AND10MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageTime.Between1And10MB";

			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWWEEN10AND20MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageSize.Between10And20MB";
			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWWEEN10AND20MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageTime.Between10And20MB";

			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_OVER20MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageSize.Over20MB";
			public static readonly string DATA_LONGTEXT_STREAM_AVERAGE_TIME_OVER20MB = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.AverageTime.Over20MB";

			public static readonly string DATA_LONGTEXT_STREAM_LARGEST_SIZE = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.LargestLongText.Size";
			public static readonly string DATA_LONGTEXT_STREAM_LARGEST_TIME = $"{_SYNC_METRIC_PREPEND}.Data.LongTextStream.LargestLongText.Time";

			public static readonly string KEPLER_SERVICE_INTERCEPTOR_PREFIX = $"{_SYNC_METRIC_PREPEND}.KeplerServiceInterceptor";
		}
	}
}