using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    public interface IJobProgressHandler
    {
        Task<IDisposable> BeginUpdateAsync(int workspaceId, Guid importJobId, int jobHistoryId);
        Task UpdateReadItemsCountAsync(Job job, CustomProviderJobDetails jobDetails);
        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);
    }
}