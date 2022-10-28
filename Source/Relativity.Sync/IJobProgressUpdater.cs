using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IJobProgressUpdater
    {
        Task SetTotalItemsCountAsync(int totalItemsCount);

        Task SetJobStartedAsync();

        Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount);

        Task UpdateJobProgressAsync(
            int workspaceId,
            int jobHistoryId,
            int completedRecordsCount,
            int failedRecordsCount);

        Task UpdateJobStatusAsync(JobHistoryStatus status);

        Task AddJobErrorAsync(Exception ex);
    }
}
