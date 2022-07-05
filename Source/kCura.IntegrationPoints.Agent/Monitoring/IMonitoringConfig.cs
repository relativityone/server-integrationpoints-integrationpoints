using System;

namespace kCura.IntegrationPoints.Agent.Monitoring
{
    public interface IMonitoringConfig
    {
        TimeSpan MemoryUsageInterval { get; }

        TimeSpan HeartbeatInterval { get; }

        TimeSpan LongRunningJobsTimeThreshold { get; }
    }
}
