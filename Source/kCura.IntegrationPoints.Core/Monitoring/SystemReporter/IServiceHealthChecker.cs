using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public interface IServiceHealthChecker
    {
        Task<bool> IsServiceHealthyAsync();
    }
}
