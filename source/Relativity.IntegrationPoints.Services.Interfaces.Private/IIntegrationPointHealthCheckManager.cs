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
        ///
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        Task<HealthCheckOperationResult> RunHealthChecksAsync();

        /// <summary>
        ///
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        Task<HealthCheckOperationResult> RunDeploymentHealthChecksAsync();
    }
}
