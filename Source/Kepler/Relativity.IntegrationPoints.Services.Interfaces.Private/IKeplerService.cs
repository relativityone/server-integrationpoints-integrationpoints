using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Interface for Kepler services in the Integration Points module.
    /// </summary>
    public interface IKeplerService
    {
        /// <summary>
        /// Pings the service to ensure it is up and running.
        /// </summary>
        /// <returns>True if the service is responsive; otherwise, false.</returns>
        Task<bool> PingAsync();
    }
}