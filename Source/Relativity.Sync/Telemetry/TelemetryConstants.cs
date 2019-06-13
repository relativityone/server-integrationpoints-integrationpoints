namespace Relativity.Sync.Telemetry
{
	internal static class TelemetryConstants
	{
		private const string _METRIC_PREPEND = "Relativity.IntegrationPoints";

		public const string TELEMETRY_CATEGORY = "Integration Points";

		public const string PROVIDER_NAME = "Sync";

		public static class MetricIdentifiers
		{
			public static readonly string JOB_START_TYPE = $"{_METRIC_PREPEND}.Job.Start.Type";
			public static readonly string JOB_END_STATUS = $"{_METRIC_PREPEND}.Job.End.Status";

			public static readonly string DATA_RECORDS_TRANSFERRED = $"{_METRIC_PREPEND}.Data.Records.Transferred";
			public static readonly string DATA_RECORDS_FAILED = $"{_METRIC_PREPEND}.Data.Records.Failed";
			public static readonly string DATA_RECORDS_TOTAL_REQUESTED = $"{_METRIC_PREPEND}.Data.Records.TotalRequested";
			public static readonly string DATA_FIELDS_MAPPED = $"{_METRIC_PREPEND}.Data.Fields.Mapped";
			public static readonly string DATA_FILES_SIZE = $"{_METRIC_PREPEND}.Data.Files.Size";
		}
	}
}