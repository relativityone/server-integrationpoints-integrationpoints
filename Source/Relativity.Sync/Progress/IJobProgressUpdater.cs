using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Progress
{
    internal interface IJobProgressUpdater
    {
        Task SetTotalItemsCountAsync(int totalItemsCount);

        Task SetJobStartedAsync();

        Task UpdateJobProgressAsync(Progress progress);

        Task UpdateJobProgressAsync(int workspaceId, int jobHistoryId, Progress progress);

        Task UpdateJobStatusAsync(JobHistoryStatus status);

        Task AddJobErrorAsync(Exception ex);
    }
}
