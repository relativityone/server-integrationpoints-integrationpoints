using System;
using kCura.IntegrationPoints.Domain.Managers;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobStopManager
{
    public interface ICancellationTokenFactory
    {
        CompositeCancellationToken CreateJobStopManager(Guid batchInstance, long jobId);
    }
}