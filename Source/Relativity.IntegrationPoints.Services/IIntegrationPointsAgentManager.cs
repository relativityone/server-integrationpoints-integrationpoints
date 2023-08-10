using Relativity.Kepler.Services;
using System.Threading.Tasks;
using System;
using WorkloadDiscovery;

namespace Relativity.IntegrationPoints.Services
{
	/// <summary>
	/// This change will be added to avoid error caused due to kepler service changes.
	/// This will be cleaned up soon and ticket created RMT-12808
	/// </summary>
	[ServiceAudience(Audience.Private)]
	public interface IIntegrationPointsAgentManager : IKeplerService, IDisposable
	{
		[HttpGet]
		Task<Workload> GetWorkloadAsync();
	}
}