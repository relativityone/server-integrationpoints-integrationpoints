using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface ISystemHealthReporter
    {
        Task<Dictionary<string, object>> GetSystemHealthStatisticsAsync();
    }
}
