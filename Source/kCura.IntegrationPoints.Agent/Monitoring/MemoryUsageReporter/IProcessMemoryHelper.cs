using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IProcessMemoryHelper
    {
        Dictionary<string, object> GetApplicationSystemStatistics();
    }
}
