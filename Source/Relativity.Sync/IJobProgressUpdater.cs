using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal enum JobHistoryStatus
    {
        Validating,
        ValidationFailed,
        Processing,
        Completed,
        CompletedWithErrors,
        Failed,
        Stopping,
        Stopped,
        Suspending,
        Suspended,
    }

    internal interface IJobProgressUpdater
    {
        Task SetTotalItemsCountAsync(int totalItemsCount);
        Task SetJobStartedAsync();
        Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount);
        Task UpdateJobStatusAsync(JobHistoryStatus status);
        Task AddJobErrorAsync(Exception ex);
    }
}