using System;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IMemoryUsageReporter : IDisposable
    {
        IDisposable ActivateTimer(int timeInterval, long jobId, string jobDetails, string jobType);
    }
}
