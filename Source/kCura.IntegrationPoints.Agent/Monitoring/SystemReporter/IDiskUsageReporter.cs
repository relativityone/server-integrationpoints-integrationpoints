using System.Collections.Generic;

namespace kCura.IntegrationPoints.Agent.Monitoring.SystemReporter
{
    public interface IDiskUsageReporter
    {
        Dictionary<string, object> GetFileShareUsage();
    }
}
