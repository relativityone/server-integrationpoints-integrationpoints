using System;

namespace kCura.IntegrationPoints.Agent.Monitoring.HearbeatReporter
{
    internal interface IHeartbeatReporter
    {
        IDisposable ActivateHeartbeat(long jobId);
    }
}
