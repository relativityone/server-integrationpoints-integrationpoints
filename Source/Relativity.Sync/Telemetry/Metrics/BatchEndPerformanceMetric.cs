using Relativity.Sync.Executors;
// ReSharper disable InconsistentNaming

namespace Relativity.Sync.Telemetry.Metrics
{
    internal class BatchEndPerformanceMetric : MetricBase<BatchEndPerformanceMetric>
    {
        public string WorkflowName => "Relativity.Sync";
        public string StageName => "Transfer";

        public long? Elapsed { get; set; }

        public string APMCategory => "PerformanceBatchJob";
        public string CorrelationID => WorkflowId;
        public int? JobID { get; set; }
        public int? WorkspaceID { get; set; }

        public ExecutionStatus JobStatus { get; set; }
        public int? RecordNumber { get; set; }
        public BatchRecordType? RecordType { get; set; }
        
        
        public double? JobSizeGB { get; set; }
        public double? JobSizeGB_Metadata { get; set; }
        public double? JobSizeGB_Files { get; set; }
        
        public int? UserID { get; set; }
        public int? SavedSearchID { get; set; }
    }
}