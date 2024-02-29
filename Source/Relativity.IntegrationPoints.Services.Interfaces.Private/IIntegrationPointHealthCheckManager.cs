using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Kepler.Services;
using Relativity.Telemetry.APM;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Manager for Health Check for Integration Points
    /// </summary>
    [WebService("Integration Point Health Check")]
    [ServiceAudience(Audience.Private)]
    public interface IIntegrationPointHealthCheckManager : IKeplerService, IDisposable
    {
        /// <summary>
        /// Runs health checks for integration points.
        /// </summary>
        /// <returns>Health check operation result.</returns>
        [HttpPost]
        Task<HealthCheckOperationResult> RunHealthChecksAsync();

        /// <summary>
        /// Runs deployment health checks for integration points.
        /// </summary>
        /// <returns>Health check operation result.</returns>
        [HttpGet]
        Task<HealthCheckOperationResult> RunDeploymentHealthChecksAsync();
    }
}
