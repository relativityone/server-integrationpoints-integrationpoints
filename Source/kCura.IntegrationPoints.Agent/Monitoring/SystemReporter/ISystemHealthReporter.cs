using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface ISystemHealthReporter
    {
        Dictionary<string, object> GetSystemHealthStatistics();
    }
}
