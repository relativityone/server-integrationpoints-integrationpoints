using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Services
{
	public interface IKeplerService
	{
		/// <summary>
		/// Pings the service to ensure it is up and running.
		/// </summary>
		Task<bool> PingAsync();
	}
}