using Relativity.Telemetry.Services.Metrics;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.RelativitySync.Metrics
{
    public interface IMetric
    {
        Task SendAsync();
    }
}
