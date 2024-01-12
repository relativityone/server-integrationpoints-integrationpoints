using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Services
{
    public interface IKeplerService
    {
        /// <summary>
        /// Pings the service to ensure it is up and running.
        /// </summary>
        Task<bool> PingAsync();
    }
}
