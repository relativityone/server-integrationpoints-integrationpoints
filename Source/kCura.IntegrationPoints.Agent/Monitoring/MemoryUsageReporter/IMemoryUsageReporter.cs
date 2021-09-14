using Relativity.API;
using System;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IMemoryUsageReporter
    {
        IDisposable ActivateTimer(int timeInterval, long jobId, string jobType, IAPILog logger);
    }
}
