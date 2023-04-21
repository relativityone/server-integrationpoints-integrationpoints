using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation
{
    public interface ICancellationTokenFactory
    {
        CompositeCancellationToken CreateJobStopManager(Guid batchInstance, long jobId);
    }
}