using System;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IMemoryUsageReporter
    {
        IDisposable ActivateTimer(long jobId, string correlationId, string jobType);
    }
}
