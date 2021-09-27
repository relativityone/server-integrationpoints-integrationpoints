using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IProcessMemoryHelper
    {
        long GetCurrentProcessMemoryUsage();

        Dictionary<string, object> GetApplicationSystemStats();
    }
}
