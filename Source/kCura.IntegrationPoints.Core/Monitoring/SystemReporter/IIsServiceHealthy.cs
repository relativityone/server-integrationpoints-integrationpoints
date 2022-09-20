using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public interface IIsServiceHealthy
    {
        Task<bool> IsServiceHealthyAsync();
    }
}
