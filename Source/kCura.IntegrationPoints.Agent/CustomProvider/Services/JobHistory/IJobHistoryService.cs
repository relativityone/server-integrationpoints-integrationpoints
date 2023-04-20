using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory
{
    public interface IJobHistoryService
    {
        Task<Data.JobHistory> ReadJobHistoryByGuidAsync(int workspaceId, Guid jobHistoryGuid);

        Task<int> CreateJobHistoryAsync(int workspaceId, Data.JobHistory jobHistory);

        Task UpdateStatusAsync(int workspaceId, int jobHistoryId, Guid statusGuid);

        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);

        Task UpdateReadItemsCountAsync(int workspaceId, int jobHistoryId, int readItemsCount);

        Task UpdateProgressAsync(int workspaceId, int jobHistoryId, int importedItemsCount, int failedItemsCount);
    }
}
