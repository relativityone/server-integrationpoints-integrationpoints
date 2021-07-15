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
		[HttpPost]
		Task<HealthCheckOperationResult> RunHealthChecksAsync();
		
		[HttpGet]
		Task<HealthCheckOperationResult> RunDeploymentHealthChecksAsync();
	}
}
