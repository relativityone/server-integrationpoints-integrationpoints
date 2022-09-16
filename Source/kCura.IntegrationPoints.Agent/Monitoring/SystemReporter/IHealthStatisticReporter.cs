using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface IHealthStatisticReporter
    {
        Task<Dictionary<string, object>> GetStatisticAsync();
    }
}
