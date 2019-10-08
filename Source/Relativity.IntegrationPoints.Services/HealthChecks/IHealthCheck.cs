using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services
{
    public interface IHealthCheck
    {
        Task<HealthCheckOperationResult> Check();
    }
}