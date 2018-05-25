using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Services
{
    public class SuccessHealthCheck : IHealthCheck
    {
        public Task<HealthCheckOperationResult> Check()
        {
            return Task.FromResult(new HealthCheckOperationResult(true, "Service is healthy"));
        }
    }
}