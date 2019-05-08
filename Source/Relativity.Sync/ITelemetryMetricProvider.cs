using System.Threading.Tasks;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync
{
	internal interface ITelemetryMetricProvider
	{
		Task AddMetricsForCategory(CategoryRef category);
	}
}