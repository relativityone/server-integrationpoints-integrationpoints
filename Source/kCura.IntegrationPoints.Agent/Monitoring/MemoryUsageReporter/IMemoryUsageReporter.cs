using System;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IMemoryUsageReporter
    {
        IDisposable ActivateTimer(long jobId, string jobDetails, string jobType);
    }
}
