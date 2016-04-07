using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;

namespace kCura.IntegrationPoints.Services
{
	/// <summary>
	/// Enables access to information about documents in the eca and investigations application
	/// </summary>
	[WebService("Document Manager")]
	[ServiceAudience(Audience.Private)]
	public interface IDocumentManager: IDisposable
	{
		/// <summary>
		/// Pings the service to ensure it is up and running.
		/// </summary>
		Task<bool> PingAsync();
	}
}