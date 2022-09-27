using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public interface ISystemHealthReporter
    {
        Task<Dictionary<string, object>> GetSystemHealthStatisticsAsync();
    }
}
