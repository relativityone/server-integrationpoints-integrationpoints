using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
    internal interface ITelemetryManager
    {
        void AddMetricProvider(ITelemetryMetricProvider metricProvider);

        Task InstallMetrics();
    }
}
