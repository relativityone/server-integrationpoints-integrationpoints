using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace Relativity.IntegrationPoints.Services
{
	[WebService("Integration Points Agent")]
	[ServiceAudience(Audience.Private)]
	public interface IIntegrationPointsAgentManager : IKeplerService, IDisposable
	{
		[HttpGet]
		Task<WorkloadDiscovery.Workload> GetWorkloadAsync();
	}
}