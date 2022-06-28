using System;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    internal class MonitoringConfig : IMonitoringConfig
    {
        public TimeSpan MemoryUsageInterval => TimeSpan.FromSeconds(30);

        public TimeSpan HeartbeatInterval => TimeSpan.FromMinutes(5);
    }
}
