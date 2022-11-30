namespace Relativity.Sync.Telemetry
{
    internal static class TelemetryConstants
    {
        public const string SYNC_TELEMETRY_CATEGORY = "Relativity Sync";
        public const string INTEGRATION_POINTS_TELEMETRY_CATEGORY = "Integration Points";

        public const string PROVIDER_NAME = "Sync";

        public const string FLOW_TYPE_SAVED_SEARCH_IMAGES = "SavedSearch.Images";
        public const string FLOW_TYPE_SAVED_SEARCH_NATIVES_AND_METADATA = "SavedSearch.NativesAndMetadata";
        public const string FLOW_TYPE_VIEW_NON_DOCUMENT_OBJECTS = "View.NonDocumentObjects";

        public static class MetricIdentifiers
        {
            public const string KEPLER_SERVICE_INTERCEPTOR_DURATION_SUFFIX = "Duration";
            public const string KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX = "Success";
            public const string KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX = "Failed";
            public const string KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX = "AuthRefresh";

            public const string APM_FLOW_NAME_IMAGES = "Images";
            public const string APM_FLOW_NAME_NATIVES_OR_METADATA = "NativesOrMetadata";
            public const string APM_FLOW_NAME_NON_DOCUMENT_OBJECTS = "NonDocumentObjects";

            public const string JOB_CORRELATION_ID = "Relativity.Sync.Job.CorrelationId";

            public const string JOB_START_TYPE = "Relativity.Sync.Job.Start.Type";
            public const string JOB_RESUME_TYPE = "Relativity.Sync.Job.Resume.Type";

            public const string JOB_SUSPENDED_STATUS_NATIVES_AND_METADATA = "Relativity.Sync.Job.Suspended.Status.SavedSearch.NativesAndMetadata";
            public const string JOB_SUSPENDED_STATUS_IMAGES = "Relativity.Sync.Job.Suspended.Status.SavedSearch.Images";
            public const string JOB_SUSPENDED_STATUS_NON_DOCUMENT_OBJECTS = "Relativity.Sync.Job.Suspended.Status.View.NonDocumentObjects";

            public const string JOB_END_STATUS_NATIVES_AND_METADATA = "Relativity.Sync.Job.End.Status.SavedSearch.NativesAndMetadata";
            public const string JOB_END_STATUS_IMAGES = "Relativity.Sync.Job.End.Status.SavedSearch.Images";
            public const string JOB_END_STATUS_NON_DOCUMENT_OBJECTS = "Relativity.Sync.Job.End.Status.View.NonDocumentObjects";

            public const string RETRY_JOB_START_TYPE = "Relativity.Sync.Retry.Job.Start.Type";
            public const string RETRY_JOB_END_STATUS = "Relativity.Sync.Retry.Job.End.Status";

            public const string FLOW_TYPE = "Relativity.Sync.FlowType";

            public const string PARENT_APPLICATION_NAME = "Relativity.Sync.ParentApplicationName";

            public const string DATA_BYTES_METADATA_TRANSFERRED = "Relativity.Sync.Data.Bytes.MetadataTransferred";
            public const string DATA_BYTES_NATIVES_TRANSFERRED = "Relativity.Sync.Data.Bytes.NativesTransferred";
            public const string DATA_BYTES_IMAGES_TRANSFERRED = "Relativity.Sync.Data.Bytes.ImagesTransferred";
            public const string DATA_BYTES_TOTAL_TRANSFERRED = "Relativity.Sync.Data.Bytes.TotalTransferred";
            public const string DATA_BYTES_NATIVES_REQUESTED = "Relativity.Sync.Data.Bytes.NativesRequested";
            public const string DATA_BYTES_IMAGES_REQUESTED = "Relativity.Sync.Data.Bytes.ImagesRequested";
            public const string DATA_RECORDS_TRANSFERRED = "Relativity.Sync.Data.Records.Transferred";
            public const string DATA_RECORDS_TAGGED = "Relativity.Sync.Data.Records.Tagged";
            public const string DATA_RECORDS_FAILED = "Relativity.Sync.Data.Records.Failed";
            public const string DATA_RECORDS_TOTAL_REQUESTED = "Relativity.Sync.Data.Records.TotalRequested";
            public const string DATA_FIELDS_MAPPED = "Relativity.Sync.Data.Fields.Mapped";
            public const string DATA_FIELDS_TOTAL_REQUESTED = "Relativity.Sync.Data.Fields.TotalRequested";
            public const string DATA_FIELDS_LINKS_MAPPED = "Relativity.Sync.Data.Fields.LinksMapped";

            public const string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_LESSTHAN1MB = "Relativity.Sync.Data.LongTextStream.AverageSize.LessThan1MB";
            public const string DATA_LONGTEXT_STREAM_AVERAGE_TIME_LESSTHAN1MB = "Relativity.Sync.Data.LongTextStream.AverageTime.LessThan1MB";

            public const string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN1AND10MB = "Relativity.Sync.Data.LongTextStream.AverageSize.Between1And10MB";
            public const string DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN1AND10MB = "Relativity.Sync.Data.LongTextStream.AverageTime.Between1And10MB";

            public const string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_BETWEEN10AND20MB = "Relativity.Sync.Data.LongTextStream.AverageSize.Between10And20MB";
            public const string DATA_LONGTEXT_STREAM_AVERAGE_TIME_BETWEEN10AND20MB = "Relativity.Sync.Data.LongTextStream.AverageTime.Between10And20MB";

            public const string DATA_LONGTEXT_STREAM_AVERAGE_SIZE_OVER20MB = "Relativity.Sync.Data.LongTextStream.AverageSize.Over20MB";
            public const string DATA_LONGTEXT_STREAM_AVERAGE_TIME_OVER20MB = "Relativity.Sync.Data.LongTextStream.AverageTime.Over20MB";

            public const string KEPLER_SERVICE_INTERCEPTOR_PREFIX = "Relativity.Sync.KeplerServiceInterceptor";

            public const string TAG_DOCUMENTS_SOURCE_UPDATE_TIME = "Relativity.Sync.TagDocuments.SourceUpdate.Time";
            public const string TAG_DOCUMENTS_SOURCE_UPDATE_COUNT = "Relativity.Sync.TagDocuments.SourceUpdate.Count";

            public const string TAG_DOCUMENTS_DESTINATION_UPDATE_TIME = "Relativity.Sync.TagDocuments.DestinationUpdate.Time";
            public const string TAG_DOCUMENTS_DESTINATION_UPDATE_COUNT = "Relativity.Sync.TagDocuments.DestinationUpdate.Count";

            public const string LONG_TEXT_STREAM_RETRY_COUNT = "Relativity.Sync.LongTextStreamBuilder.Retry.Count";
        }
    }
}
