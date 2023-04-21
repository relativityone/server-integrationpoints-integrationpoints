using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobCancellation
{
    public interface ICancellationTokenFactory
    {
        CompositeCancellationToken GetCancellationToken(Guid batchInstance, long jobId);
    }
}