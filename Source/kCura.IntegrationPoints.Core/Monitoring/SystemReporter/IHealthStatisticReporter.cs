using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Monitoring.SystemReporter
{
    public interface IHealthStatisticReporter
    {
        Task<Dictionary<string, object>> GetStatisticAsync();
    }
}
