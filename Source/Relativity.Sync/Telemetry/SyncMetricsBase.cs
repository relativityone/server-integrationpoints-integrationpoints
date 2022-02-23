namespace Relativity.Sync.Telemetry
{
    internal abstract class SyncMetricsBase
    {
        public abstract ISyncMetrics SyncMetricsValue { get; set; }
    }
}
