using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
    public interface ITelemetryMetricProvider
    {
        void Run(Category integrationPointCategory, IHelper helper);
    }
}
