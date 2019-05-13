using System.Threading.Tasks;

namespace Relativity.Sync.Telemetry
{
	internal interface ITelemetryManager
	{
		void AddMetricProviders(ITelemetryMetricProvider metricProvider);
		Task InstallMetrics();
	}
}