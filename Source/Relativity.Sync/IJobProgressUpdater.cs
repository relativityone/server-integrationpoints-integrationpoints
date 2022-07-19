using System.Threading.Tasks;

namespace Relativity.Sync
{
    internal interface IJobProgressUpdater
    {
        Task SetTotalItemsCountAsync(int totalItemsCount);
        Task UpdateJobProgressAsync(int completedRecordsCount, int failedRecordsCount);
    }
}