using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    public interface IJobProgressHandler
    {
        Task<IDisposable> BeginUpdateAsync(int workspaceId, Guid importJobId, int jobHistoryId);
    }
}