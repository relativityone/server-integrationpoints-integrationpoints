using kCura.IntegrationPoints.Config;
using System;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    internal class MonitoringConfig : IMonitoringConfig
    {
        private readonly IConfig _instanceSettings;

        public MonitoringConfig(IConfig instanceSettings)
        {
            _instanceSettings = instanceSettings;
        }

        public TimeSpan TimerStartDelay => TimeSpan.Zero;

        public TimeSpan MemoryUsageInterval => TimeSpan.FromSeconds(30);

        public TimeSpan HeartbeatInterval => TimeSpan.FromMinutes(5);

        public TimeSpan LongRunningJobsTimeThreshold => _instanceSettings.RunningJobTimeThreshold;
    }
}
