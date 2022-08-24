using System;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    public interface IMonitoringConfig
    {
        TimeSpan TimerStartDelay { get; }

        TimeSpan MemoryUsageInterval { get; }

        TimeSpan HeartbeatInterval { get; }

        TimeSpan LongRunningJobsTimeThreshold { get; }
    }
}
