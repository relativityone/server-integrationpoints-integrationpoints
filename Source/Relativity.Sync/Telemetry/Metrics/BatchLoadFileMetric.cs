namespace Relativity.Sync.Telemetry.Metrics
{
    internal sealed class BatchLoadFileMetric : MetricBase<BatchLoadFileMetric>
    {
        public string Status { get; set; }

        public int TotalRecordsRequested { get; set; }

        public int TotalRecordsRead { get; set; }

        public int TotalRecordsReadFailed { get; set; }

        public long ReadMetadataBytesSize { get; set; }

        public double WriteLoadFileDuration { get; set; }
    }
}
