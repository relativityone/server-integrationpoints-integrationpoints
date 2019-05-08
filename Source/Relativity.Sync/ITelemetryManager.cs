using System.Threading.Tasks;

namespace Relativity.Sync
{
	internal interface ITelemetryManager
	{
		void AddMetricProviders(ITelemetryMetricProvider metricProvider);
		Task InstallMetrics();
	}
}