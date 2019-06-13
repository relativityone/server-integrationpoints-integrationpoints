using System.Threading.Tasks;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal interface ITelemetryMetricProvider
	{
		Task AddMetricsForCategory(IInternalMetricsCollectionManager metricsCollectionManager, CategoryRef category);
	}
}