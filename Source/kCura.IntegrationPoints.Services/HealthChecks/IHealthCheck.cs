using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
{
    public interface IHealthCheck
    {
        Task<HealthCheckOperationResult> Check();
    }
}