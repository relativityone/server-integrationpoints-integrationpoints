using System.Threading.Tasks;
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Executors
{
    internal interface IItemLevelErrorHandler
    {
        void Initialize(IItemStatusMonitor statusMonitor, IBatch batch);

        void HandleItemLevelError(long completedItem, ItemLevelError itemLevelError);

        Task HandleDataSourceProcessingFinishedAsync(IBatch batch);
    }
}
