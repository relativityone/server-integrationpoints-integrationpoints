using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services.Interfaces.Private
{
	/// <summary>
	/// Enables access to job history information
	/// </summary>
	[WebService("Job History Manager")]
	[ServiceAudience(Audience.Public)]
	public interface IIntegrationPointsManager : IDisposable
    {
		/// <summary>
		/// Pings the service to ensure its up and running
		/// </summary>
		Task<bool> Ping();
	}
}
