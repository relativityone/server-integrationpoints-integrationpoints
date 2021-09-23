using Relativity.API;
using System;
using System.Reactive.Concurrency;

namespace kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter
{
    public interface IMemoryUsageReporter
    {
        IDisposable ActivateTimer(int timeInterval, long jobId, string jobDetails, string jobType, IScheduler scheduler = null);
    }
}
